using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
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
            SpaceRatioThreshold = 0.15d;
            WordPixelSpacesRatio = 0.01d;
            Characters = new ObservableCollection<Rect>();
        }

        public EdgeDetector EdgeDetector
        {
            get;
            set;
        }

        public ObservableCollection<Rect> Characters
        {
            get;
            set;
        }

        public double WordPixelSpacesRatio
        {
            get;
            set;
        }

        public double SpaceRatioThreshold
        {
            get;
            set;
        }

        public override unsafe void Initialize()
        {
            EdgeDetector.Compute();
            base.Initialize();
        }

        protected override unsafe void OnCompute()
        {
            Output.WritePixels(new Int32Rect(0, 0, Source.PixelWidth, Source.PixelHeight), m_readBuffer, m_stride, 0);
            // isolate lines
            var currentLine = new Int32Rect();
            bool inLine = false;
            var lines = new List<Int32Rect>();
            for (int y = 0; y < Source.PixelHeight; y++)
            {
                int sum = 0;
                for (int x = 0; x < Source.PixelWidth; x++)
                {
                    if (EdgeDetector.Edges[x, y])
                    {
                        sum++;
                    }
                }

                var ratio = sum/(double)Source.PixelWidth;
                bool blank = ratio < SpaceRatioThreshold;

                if (!blank && !inLine)
                {
                    inLine = true;
                    currentLine = new Int32Rect(0, y, Source.PixelWidth, 0);
                }

                if (blank && inLine)
                {
                    currentLine.Height = y - currentLine.Y;
                    inLine = false;
                    lines.Add(currentLine);
                    Output.DrawRectangle(currentLine.X, currentLine.Y, currentLine.X + currentLine.Width, currentLine.Y + currentLine.Height, Colors.Red);
                }
            }

            // isolate words
            foreach (var line in lines)
            {
                var right = line.X + line.Width;
                var bot = line.Y + line.Height;
                int spaces = 0;
                bool inWord = false;
                var currentWord = new Int32Rect();
                var words = new List<Int32Rect>();
                for (int x = line.X; x < right; x++)
                {
                    int sum = 0;
                    int y = line.Y;
                    for (;y < bot; y++)
                    {
                        if (EdgeDetector.Edges[x, y])
                        {
                            sum++;
                        }
                    }

                    var ratio = sum/(double)line.Height;
                    bool blank = ratio < SpaceRatioThreshold;

                    if (blank)
                        spaces++;

                    if (!blank && !inWord)
                    {
                        spaces = 0;
                        inWord = true;
                        currentWord = new Int32Rect(x, line.Y, 0, line.Height);
                    }
                    else
                    {
                        bool newWord = (double)spaces/line.Width > WordPixelSpacesRatio;
                        if (newWord)
                        {
                            inWord = false;
                            currentWord.Width = x - spaces - currentWord.X;
                            words.Add(currentWord);
                            Output.DrawRectangle(currentWord.X, currentWord.Y, currentWord.X + currentWord.Width, currentWord.Y + currentWord.Height, Colors.Blue);
                        }
                    }
                }
            }
        }
    }
}