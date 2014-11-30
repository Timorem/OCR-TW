using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OCR.WPF.Algorithms
{
    public class CharacterIsolation : AlgorithmBase
    {
        public CharacterIsolation(BitmapSource image)
            : base(image)
        {
            EdgeDetector = new EdgeDetector(Source);
            EdgeDetector.GradientLimit = 20d;
            SpaceRatioThreshold = 0.08d;
            WordPixelSpacesRatio = 0.002d;
            CharactersPixelSpacesRatio = 0.0001d;
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

        public double WordPixelSpacesRatio
        {
            get;
            set;
        }
        public double CharactersPixelSpacesRatio
        {
            get;
            set;
        }

        public double SpaceRatioThreshold
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
            foreach (Int32Rect line in Isolate(imageRect, 0, WordPixelSpacesRatio, true, Colors.Red))
            {
                Lines.Add(line);
            }

            // isolate words
            foreach (Int32Rect line in Lines)
            {
                foreach (Int32Rect region in Isolate(line, SpaceRatioThreshold, WordPixelSpacesRatio, false, null))
                {
                    Words.Add(new Word(region));
                }
            }

            // isolate characters
            foreach (Word word in Words)
            {
                foreach (Int32Rect region in Isolate(word.Region, SpaceRatioThreshold, CharactersPixelSpacesRatio, false, Colors.Green))
                {
                    word.Characters.Add(region);
                }
            }
        }

        private IEnumerable<Int32Rect> Isolate(Int32Rect zone, double spacesRatio, double blanksBetweenRegionsRatio, bool horizontal, Color? drawColor)
        {
            int right = zone.X + zone.Width;
            int bot = zone.Y + zone.Height;
            int spaces = 0;
            bool inRegion = false;
            var currentRegion = new Int32Rect();
            int x = horizontal ? zone.Y : zone.X;
            for (; x < (horizontal ? bot : right); x++)
            {
                int sum = 0;
                int y = horizontal ? zone.X : zone.Y;
                for (; y < (horizontal ? right : bot); y++)
                {
                    if (horizontal ? EdgeDetector.Edges[y, x] : EdgeDetector.Edges[x, y])
                        sum++;
                }

                double ratio = (double) sum/(horizontal ? zone.Width : zone.Height);
                bool blank = ratio <= spacesRatio;

                if (blank)
                    spaces++;

                if (!blank && !inRegion)
                {
                    spaces = 0;
                    inRegion = true;
                    currentRegion = horizontal ? new Int32Rect(zone.X, x, zone.Width, 0) : new Int32Rect(x, zone.Y, 0, zone.Height);
                }
                else if (inRegion)
                {
                    bool newRegion = (double) spaces/(horizontal ? zone.Height : zone.Width) >= blanksBetweenRegionsRatio;
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
        }
    }
}
