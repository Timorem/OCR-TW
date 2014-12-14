using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Imaging;

namespace OCR.WPF.Algorithms
{
    public class CharacterIsolation : AlgorithmBase
    {
        public CharacterIsolation(BitmapSource image, EdgeDetector edgeDetector)
            : base(image)
        {
            EdgeDetector = edgeDetector;
            LinesBlankThreshold = 0.001d;
            CharactersBlankThreshold = 0.08d;
            WordPixelSpaces = 5;
            CharactersPixelSpaces = 1;
            Characters = new ObservableCollection<Int32Rect>();
            Words = new ObservableCollection<Word>();
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

        public ObservableCollection<Int32Rect> Characters
        {
            get;
            set;
        }

        public ObservableCollection<Word> Words
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

        public override void Initialize()
        {
            EdgeDetector.Compute();
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

            // isolate words
            foreach (Int32Rect line in Lines)
            {
                foreach (Int32Rect region in Isolate(line, CharactersBlankThreshold, WordPixelSpaces, false, ShowWords ? (Color?)Colors.Blue : null))
                {
                    if (region.Height > 0 && region.Width > 0)
                        Words.Add(new Word(region));
                }
            }

            // isolate characters
            foreach (Word word in Words)
            {
                foreach (Int32Rect region in Isolate(word.Region, CharactersBlankThreshold, CharactersPixelSpaces, false, ShowCharacters ? (Color?)Colors.Green : null, true))
                {
                    if (region.Height > 0 && region.Width > 0)
                        word.Characters.Add(region);
                }
            }
        }       

        private IEnumerable<Int32Rect> Isolate(Int32Rect zone, double spacesRatio, int blanksBetweenRegionsRatio, bool horizontal, Color? drawColor, bool drawBlanks = false)
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
                int y = horizontal ? zone.X : zone.Y;
                for (; y < (horizontal ? right : bot); y++)
                {
                    sum += horizontal ? GetPixel(y, x).GetBrightness() : GetPixel(x, y).GetBrightness();
                }

                double ratio = 1-(double) sum/(horizontal ? zone.Width : zone.Height);
                bool blank = ratio <= spacesRatio;

                if (blank)
                {
                    if (!horizontal && drawBlanks)
                        Output.DrawLine(x, zone.Y, x, bot, Colors.HotPink);
                    spaces++;
                }

                if (!blank && !inRegion)
                {
                    spaces = 0;
                    inRegion = true;
                    currentRegion = horizontal ? new Int32Rect(zone.X, x, zone.Width, 0) : new Int32Rect(x, zone.Y, 0, zone.Height);
                }
                else if (inRegion && (blank || x == (horizontal ? bot : right) -1 ))
                {
                    bool newRegion = spaces >= blanksBetweenRegionsRatio;
                    if (newRegion)
                    {
                        inRegion = false;
                        if (horizontal)
                            currentRegion.Height = x - spaces - currentRegion.Y;
                        else
                            currentRegion.Width = x - spaces - currentRegion.X;

                        yield return currentRegion;

                        if (drawColor.HasValue)
                            Output.DrawRectangle(currentRegion.X, currentRegion.Y, currentRegion.X + currentRegion.Width, currentRegion.Y + currentRegion.Height, drawColor.Value);
                    }
                }
            }
            if (inRegion)
                yield return currentRegion;
        }
    }
}
