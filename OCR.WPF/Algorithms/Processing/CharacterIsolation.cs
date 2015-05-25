using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Algorithms.PreProcessing;
using OCR.WPF.Imaging;

namespace OCR.WPF.Algorithms.Processing
{
    public class CharacterIsolation : AlgorithmBase
    {

        /// <summary>
        /// Algorithme qui sépare les caractères les uns des autres
        /// </summary>
        /// <param name="image"></param>
        public CharacterIsolation(BitmapSource image)
            : base(image)
        {
            PixelBlankThreshold = 0.001d;
            WordPixelSpaces = 3;
            CharactersPixelSpaces = 1;
            Words = new ObservableCollection<DetectedWord>();
            Lines = new ObservableCollection<Int32Rect>();
        }

        // tableau de lignes
        public ObservableCollection<Int32Rect> Lines
        {
            get;
            set;
        }

        // tableau de mots (chaque mot possède des caractères)
        public ObservableCollection<DetectedWord> Words
        {
            get;
            set;
        }

        // nombre minimum de pixels entre deux mots
        public int WordPixelSpaces
        {
            get;
            set;
        }

        // nombre de pixels entre deux caractètres
        public int CharactersPixelSpaces
        {
            get;
            set;
        }

        // seuil a partir du quel un pixel est considéré blanc
        public double PixelBlankThreshold
        {
            get;
            set;
        }

        // dessine les délimitations des lignes
        public bool ShowLines
        {
            get;
            set;
        }
        
        // dessine les délimitations des mots
        public bool ShowWords
        {
            get;
            set;
        }
        
        // dessine les délimitations des caractères
        public bool ShowCharacters
        {
            get;
            set;
        }


        // Execute l'algorithme
        protected override void OnCompute()
        {
            // initialisation
            Words.Clear();
            Lines.Clear();

            var imageRect = new Int32Rect(0, 0, Source.PixelWidth, Source.PixelHeight);
            Output.WritePixels(imageRect, m_readBuffer, m_stride, 0);

            // isole les lignes
            foreach (var line in Isolate(imageRect, WordPixelSpaces, true, ShowLines ? (Color?)Colors.Red : null))
            {                    
                if (line.Height > 0 && line.Width > 0)
                    Lines.Add(line.ToInt32Rect());
            }

            int i = 0;
            // isole les mots
            foreach (Int32Rect line in Lines)
            {
                foreach (var region in Isolate(line, WordPixelSpaces, false, ShowWords ? (Color?)Colors.Blue : null))
                {
                    if (region.Height > 0 && region.Width > 0)
                        Words.Add(new DetectedWord(region.ToInt32Rect(),i));
                }
                i++;
            }

            // isole les caractères
            foreach (DetectedWord word in Words)
            {
                foreach (var region in Isolate(word.Region, CharactersPixelSpaces, false, ShowCharacters ? (Color?)Colors.Green : null))
                {
                    if (region.Height > 0 && region.Width > 0)
                        word.Characters.Add(region.ToInt32Rect());
                }
            }
        }

        // scanne une ligne ou colonne, établit si elle est majoritairement blanche et sinon retourne le premier et dernier pixel noir
        private bool ScanLineOrColumn(bool horizontal, int index, int start, int end, out Line line)
        {
            int? startCrop = null;
            int? endCrop = null;
            int sum = 0;
            for (int subIndex = start; subIndex <= end; subIndex++)
            {
                int x = horizontal ? subIndex : index;
                int y = horizontal ? index : subIndex;

                // luminosité du pixel
                var brightness = GetPixel(x, y).GetBrightness();
                // true si le pixel est considéré noir
                var isBlack = brightness < PixelBlankThreshold;

                if (isBlack) // increment le compteur de pixels noirs
                    sum++;

                // premier pixel noir
                if (isBlack && startCrop == null)
                    startCrop = subIndex;
                
                // dernier pixel noir jusqu'a present
                if (isBlack)
                    endCrop = subIndex;
            }
            line = new Line(index, startCrop ?? start, endCrop ?? end, horizontal);
            return sum == 0;
        }

        private IEnumerable<Region> Isolate(Int32Rect zone, int blanksBetweenRegions, bool horizontal, Color? drawColor)
        {
            // ------ initialisation -----

            // on etudie dans le cadre délimité par la zone
            // le pixel en (right, bot) est le dernier pixel de la zone
            int right = zone.X + zone.Width - 1;
            int bot = zone.Y + zone.Height - 1;

            int spaces = 0;
            bool inRegion = false;
            Region currentRegion = null;

            int index = horizontal ? zone.Y : zone.X;
            int lastIndex = (horizontal ? bot : right);
            // -------------------------

            // ----- boucle sur chaque ligne/colonne ----
            for (; index <= lastIndex; index++) // dernier indice compris
            {
                var start = horizontal ? zone.X : zone.Y;
                var end = horizontal ? right : bot;

                // line delimite la zones de pixels noirs
                Line line;
                var isBlank = ScanLineOrColumn(horizontal, index, start, end, out line);

                if (isBlank) // c'est un espace blanc
                    spaces++;
                else
                    spaces = 0; // ligne noire on réinitialise le compteur


                // réadapte la taille de la zone si on a rencontré une ligne non blanche
                if (inRegion && !isBlank)
                {
                    currentRegion.AdjustSizeToFit(line);
                }

                // on sort d'une region ou c'est la derniere region
                if (inRegion && (spaces >= blanksBetweenRegions || index == lastIndex))
                {
                    // on retourne cette region
                    yield return currentRegion;

                    // dessine la region
                    if (drawColor.HasValue)
                        Output.DrawRectangle(currentRegion.X, currentRegion.Y, currentRegion.X + currentRegion.Width - 1, currentRegion.Y + currentRegion.Height - 1, drawColor.Value);

                    // on reinitialise le compteur d'espaces blancs
                    spaces = 0;
                    inRegion = false; // on n'est plus dans une region
                    currentRegion = null;
                }

                if (!inRegion && !isBlank) // on entre dans une nouvelle region
                {
                    inRegion = true;
                    currentRegion = new Region(line); // nouvelle region qui continent la ligne         
                }
            }
        }
        
        // retourne le caractère qui contient le pixel donné depuis ses coordonnées
        public Int32Rect? GetCurrentZone(int x, int y)
        {
            foreach (var word in Words)
            {
                if (word.Region.X <= x && word.Region.X + word.Region.Width >= x &&
                    word.Region.Y <= y && word.Region.Y + word.Region.Height >= y)
                {
                    foreach (var character in word.Characters)
                    {
                        if (character.X <= x && character.X + character.Width >= x &&
                            character.Y <= y && character.Y + character.Height >= y)
                        {
                            return character;
                        }
                    }
                }
            }

            return null;
        } 

        
        private class Region
        {
            public Region(Line line)
            {
                X = line.StartX;
                Y = line.StartY;
                Right = line.EndX;
                Bottom = line.EndY;
            }

            public int X
            {
                get;
                set;
            }

            public int Y
            {
                get;
                set;
            }

            public int Bottom
            {
                get;
                set;
            }

            public int Right
            {
                get;
                set;
            }

            public int Width
            {
                get { return Right - X + 1; }
            }

            public int Height
            {
                get { return Bottom - Y + 1; }
            }

            public void AdjustSizeToFit(Line line)
            {
                if (line.StartX < X)
                    X = line.StartX;
                if (line.StartY < Y)
                    Y = line.StartY;
                if (line.EndX > Right)
                    Right = line.EndX;
                if (line.EndY > Bottom)
                    Bottom = line.EndY;
            }

            public Int32Rect ToInt32Rect()
            {
                return new Int32Rect(X, Y, Width, Height);
            }
        }

        private class Line
        {
            public Line(int index, int startSubIndex, int endSubIndex, bool horizontal)
            {
                if (horizontal)
                {
                    StartX = startSubIndex;
                    EndX = endSubIndex;
                    StartY = EndY = index;
                }
                else
                {
                    StartY = startSubIndex;
                    EndY = endSubIndex;
                    StartX = EndX = index;
                }
            }

            public int StartX
            {
                get;
                set;
            }

            public int StartY
            {
                get;
                set;
            }

            public int EndX
            {
                get;
                set;
            }

            public int EndY
            {
                get;
                set;
            }
        }
    }

}
