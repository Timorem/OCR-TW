using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Imaging;

namespace OCR.WPF.Algorithms.Processing
{
    /// <summary>
    /// Algorithme qui identifie les caractères
    /// </summary>
    public class CharacterRecognition : AlgorithmBase
    {
        /// <summary>
        /// Cache pour une optimisation de temps
        /// </summary>
        private struct CacheEntry
        {
            private Typeface m_font;
            private int m_width;
            private int m_height;

            public CacheEntry(Typeface font, int width, int height)
            {
                m_font = font;
                m_width = width;
                m_height = height;
            }
        }

        private static char[] PRINTEABLE_CHARS =
            new char[]
            {
                //'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };


        private static Dictionary<CacheEntry, CharacterMapEntry[]> m_fontCache =
            new Dictionary<CacheEntry, CharacterMapEntry[]>(); 

        public CharacterRecognition(BitmapSource image)
            : base(image)
        {
            Entries = new ObservableCollection<ComparaisonEntry>();
            TrustLevelThreshold = 0.40;
        }

        public Int32Rect CharacterZone
        {
            get;
            set;
        }

        public Typeface Typeface
        {
            get;
            set;
        }

        public double TrustLevel
        {
            get;
            set;
        }

        public char RecognizedCharacter
        {
            get;
            set;
        }

        public double TrustLevelThreshold
        {
            get;
            set;
        }

        public ObservableCollection<ComparaisonEntry> Entries
        {
            get;
            set;
        }

        /// <summary>
        /// Execute l'algorithme de reconnaissance
        /// </summary>
        protected override void OnCompute()
        {
            // initialisation
            Entries.Clear();
            ComparaisonEntry max = null;
            double maxLevel = 0;

            var inputProportions = (double)CharacterZone.Width/CharacterZone.Height;
            // pour chaque caractère de la police
            foreach (var entry in GetCharactersMap(Typeface, CharacterZone.Width, CharacterZone.Height))
            {
                byte[] difference = new byte[entry.BitmapBuffer.Length];

                // On constitue une image difference entre l'image d'entrée et l'image correspondant au caractère comparé
                int sum = 0;
                int sumTotal = 0;
                for (int x = 0; x < CharacterZone.Width; x++)
                {
                    for (int y = 0; y < CharacterZone.Height; y++)
                    {
                        var pixelA = GetPixel(CharacterZone.X + x, CharacterZone.Y + y);
                        var pixelB = GetPixel(entry.BitmapBuffer, x, y, CharacterZone.Width, 4);

                        // si le pixel n'est pas blanc (brightness = 1) on le compte
                        if (pixelA.GetBrightness() < 0.90)
                        {
                            // l'autre pixel n'est pas blanc non plus, ils correspondent
                            if (pixelB.GetBrightness() < 0.90)
                                sum++;

                            sumTotal++;
                        }

                        // créer l'image différence
                        SetPixel(difference, x, y, pixelA - pixelB, CharacterZone.Width, 4);
                    }
                }

                //ratio du nombre de pixels correspondant
                var ratio = sum/(double)sumTotal;
                // difference des proportions des images (largeur/hauteur)
                var diff = Math.Abs(inputProportions - entry.Proportion);
                // on défini un niveau de confiance qui prend en compte le nombre de pixels correspondants et les écarts de proportions d'images
                // ainsi des caractères qui n'ont pas les même tailles sont éliminés
                var trustLevel = Math.Max(0, ratio*(1 - diff));
                var compEntry = new ComparaisonEntry(Source, CharacterZone, entry.Character, entry.BitmapBuffer, difference, trustLevel);
                Entries.Add(compEntry);

                // comparaison du plus grand score
                if (compEntry.TrustLevel > maxLevel)
                {
                    max = compEntry;
                    maxLevel = compEntry.TrustLevel;
                }
            }

            TrustLevel = max != null ? max.TrustLevel : 0;
            // si le niveau de confiance est trop base on estime que le caractère n'est pas reconnu, on assigne un '?' à la place
            RecognizedCharacter = TrustLevel > TrustLevelThreshold ? max.Character : '?';
        }

        // donne la couleur d'un pixel de l'image donné a partir de sa mémoire tampon
        private static Color GetPixel(byte[] buffer, int x, int y, int width, int pixelSize)
        {
            var i = y*width*pixelSize + (x*pixelSize);
            return Color.FromArgb(buffer[i+3], buffer[i+2], buffer[i+1], buffer[i]);
        }

        // modifie la couleur d'un pixel
        private static void SetPixel(byte[] buffer, int x, int y, Color color, int width, int pixelSize)
        {
            var i = y*width*pixelSize + (x*pixelSize);
            buffer[i + 3] = color.A;
            buffer[i + 2] = color.R;
            buffer[i + 1] = color.G;
            buffer[i] = color.B;
        }

        /// <summary>
        /// Créer un mappage des caractères sous forme d'image pour la police et taille donnée
        /// </summary>
        /// <param name="typeface"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static CharacterMapEntry[] GetCharactersMap(Typeface typeface, int width, int height)
        {
            var param = new CacheEntry(typeface, width, height);

            if (m_fontCache.ContainsKey(param))
                return m_fontCache[param];

            var result = new CharacterMapEntry[PRINTEABLE_CHARS.Length];

            // pour chaque caractère possible ...
            int i = 0;
            foreach (var character in PRINTEABLE_CHARS)
            {
                double proportion;
                var visual = new DrawingVisual();
                using (DrawingContext drawingContext = visual.RenderOpen())
                {
                    // taille usuelle d'un caractère (ex : 12 par défaut sur word)
                    // on fait en sorte de mettre le plus gros caractère dans le cadre donné
                    var size = height*96.0/72.0;
                    var text = new FormattedText(character.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, size, Brushes.White);
                    var geom = text.BuildGeometry(new Point(0, 0));
                    // dessine un fond blanc
                    drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                    
                    // on enleve les contours blancs
                    var translateTransform = new TranslateTransform();
                    translateTransform.X = -geom.Bounds.Left;
                    translateTransform.Y = -geom.Bounds.Top;
                    proportion = geom.Bounds.Width/geom.Bounds.Height;
                    
                    // applique une homotétie pour adapter le caractère à la taille imposé
                    var scaleTransform = new ScaleTransform(width/geom.Bounds.Width, height/geom.Bounds.Height);
                    drawingContext.PushTransform(scaleTransform);
                    drawingContext.PushTransform(translateTransform);

                    // dessine le caractère en noir avec les transformations précédentes
                    drawingContext.DrawGeometry(Brushes.Black, null, geom);
                }

                // dessine le caractère sur une image
                var resizedImage = new RenderTargetBitmap(
                    width, height, // Resized dimensions
                    96, 96, // Default DPI values
                    PixelFormats.Default); // Default pixel format
                resizedImage.Render(visual);

                var stride = 4*width;
                var buffer = new byte[stride*height];
                // on récupère les pixels de l'image pour les comparer
                resizedImage.CopyPixels(buffer, stride, 0);

                result[i++] = new CharacterMapEntry(character, buffer, proportion);
            }

            // pour réduire la mémoire
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            m_fontCache.Add(param, result);
            return result;
        }
    }
}