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
        public CharacterIsolation(BitmapSource image, EdgeDetector edgeDetector)
            : base(image)
        {
            EdgeDetector = edgeDetector;
            LinesBlankThreshold = 0.001d;
            WordsBlankThreshold = 0.0055d;
            CharactersBlankThreshold = 0.566d;
            WordPixelSpaces = 3;
            CharactersPixelSpaces = 1;
            Words = new ObservableCollection<DetectedWord>();
            Lines = new ObservableCollection<Int32Rect>();
        }

        public EdgeDetector EdgeDetector
        {
            get;
            set;
        }

        public ObservableCollection<Int32Rect> Lines
        {
            get;
            set;
        }

        public ObservableCollection<DetectedWord> Words
        {
            get;
            set;
        }

        public int WordPixelSpaces
        {
            get;
            set;
        }
        public int CharactersPixelSpaces
        {
            get;
            set;
        }

        public double LinesBlankThreshold
        {
            get;
            set;
        }
        public double WordsBlankThreshold
        {
            get;
            set;
        }

        public double CharactersBlankThreshold
        {
            get;
            set;
        }

        public bool ShowLines
        {
            get;
            set;
        }

        public bool ShowWords
        {
            get;
            set;
        }

        public bool ShowCharacters
        {
            get;
            set;
        }

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

        public override void Initialize()
        {
            //EdgeDetector.Compute();
            base.Initialize();
        }

        protected override void OnCompute()
        {
            Words.Clear();
            Lines.Clear();

            var imageRect = new Int32Rect(0, 0, Source.PixelWidth, Source.PixelHeight);
            Output.WritePixels(imageRect, m_readBuffer, m_stride, 0);
            // isolate lines
            foreach (Int32Rect line in Isolate(imageRect, LinesBlankThreshold, WordPixelSpaces, true, ShowLines ? (Color?)Colors.Red : null))
            {                    
                if (line.Height > 0 && line.Width > 0)
                    Lines.Add(line);
            }

            int i = 0;
            // isolate words
            foreach (Int32Rect line in Lines)
            {
                foreach (Int32Rect region in Isolate(line, WordsBlankThreshold, WordPixelSpaces, false, ShowWords ? (Color?)Colors.Blue : null))
                {
                    if (region.Height > 0 && region.Width > 0)
                        Words.Add(new DetectedWord(region,i));
                }
                i++;
            }

            // isolate characters
            foreach (DetectedWord word in Words)
            {
                foreach (Int32Rect region in Isolate(word.Region, CharactersBlankThreshold, CharactersPixelSpaces, false, ShowCharacters ? (Color?)Colors.Green : null, true, true, true))
                {
                    if (region.Height > 0 && region.Width > 0)
                        word.Characters.Add(region);
                }
            }
        }       

        private IEnumerable<Int32Rect> Isolate(Int32Rect zone, double spacesRatio, int blanksBetweenRegionsRatio, bool horizontal, Color? drawColor, bool drawBlanks = false, bool minStrat = false, bool crop = false)
        {
            int right = zone.X + zone.Width;
            int bot = zone.Y + zone.Height;
            int spaces = 0;
            bool inRegion = false;
            var currentRegion = new Int32Rect();
            int x = horizontal ? zone.Y : zone.X;
            for (; x < (horizontal ? bot : right); x++)
            {
                double sum = 0;
                double min = 1.0;
                int y = horizontal ? zone.X : zone.Y;
                int? cropTop = null;
                int? cropBot = null;
                for (; y < (horizontal ? right : bot); y++)
                {
                    var brightness = horizontal ? GetPixel(y, x).GetBrightness() : GetPixel(x, y).GetBrightness();
                    sum += brightness;
                    if (brightness < min)
                        min = brightness;

                    if (crop && brightness < spacesRatio && cropTop == null)
                        cropTop = y;
                    if (crop && brightness >= spacesRatio && cropBot == null && cropTop != null)
                        cropBot = y;
                    if (crop && brightness < spacesRatio && cropTop != null && cropBot != null)
                        cropBot = y;
                }

                if (cropTop != null & cropBot == null)
                    cropBot = (horizontal ? right : bot) - 1;

                double ratio = 1-(double) sum/(horizontal ? zone.Width : zone.Height);
                bool blank = minStrat ? min > spacesRatio : ratio <= spacesRatio;

                if (blank)
                {
                    spaces++;
                }
                else
                    spaces = 0;

                if (!blank && !inRegion)
                {
                    spaces = 0;
                    inRegion = true;
                    if (horizontal)
                    {
                        var regionX = crop && cropTop.HasValue ? cropTop.Value : zone.X;
                        var regionWidth = crop && cropBot.HasValue ? cropBot.Value - regionX : zone.Width;
                        currentRegion = new Int32Rect(regionX, x, regionWidth, 0);
                    }
                    else
                    {
                        var regionY = crop && cropTop.HasValue ? cropTop.Value : zone.Y;
                        var regionHeight = crop && cropBot.HasValue ? cropBot.Value - regionY : zone.Height;
                        currentRegion = new Int32Rect(x, regionY, 0, regionHeight);
                    }
                }
                else if (inRegion && (blank || x == (horizontal ? bot : right) -1 ))
                {
                    bool newRegion = spaces >= blanksBetweenRegionsRatio || x == (horizontal ? bot : right) -1; // last region
                    if (newRegion)
                    {
                        inRegion = false;
                        if (horizontal)
                            currentRegion.Height = x - (spaces - 1) - currentRegion.Y;
                        else
                            currentRegion.Width = x - (spaces - 1) - currentRegion.X;

                        yield return currentRegion;

                        if (drawColor.HasValue)
                            Output.DrawRectangle(currentRegion.X, currentRegion.Y, currentRegion.X + currentRegion.Width, currentRegion.Y + currentRegion.Height, drawColor.Value);
                    }
                }

                if (inRegion && crop)
                {
                    if (cropBot.HasValue && currentRegion.Y + currentRegion.Height < cropBot.Value)
                        currentRegion.Height = cropBot.Value - currentRegion.Y;
                    if (cropTop.HasValue && currentRegion.Y > cropTop.Value)
                    {
                        var bottom = currentRegion.Y + currentRegion.Height;
                        currentRegion.Y = cropTop.Value;
                        currentRegion.Height = bottom - currentRegion.Y;
                    }

                }
            }
            if (inRegion)
                yield return currentRegion;
        }
    }
}
