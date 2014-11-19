using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Imaging;

namespace OCR.WPF.Algorithms
{
    public class EdgeDetector : AlgorithmBase
    {
        private double m_gradientCoefficient;

        public EdgeDetector(BitmapSource image)
            : base(image)
        {
        }

        public double GradientLimit
        {
            get;
            set;
        }

        public double[,] Gradients
        {
            get;
            private set;
        }

        public bool[,] Edges
        {
            get;
            private set;
        }

        public override unsafe void Initialize()
        {
            var grayScaling = new GrayScaling(Input);
            grayScaling.Compute();
            Source = grayScaling.Output;

            base.Initialize();
        }

        protected override void OnCompute()
        {
            Gradients = new double[Source.PixelWidth,Source.PixelHeight];
            Edges = new bool[Source.PixelWidth,Source.PixelHeight];
            double max = 0d;
            for (int y = 0; y < Source.PixelHeight - 2; y++)
            {
                for (int x = 0; x < Source.PixelWidth - 2; x++)
                {
                    var pixelXY = GetPixel(x, y);
                    var pixelXXY = GetPixel(x + 1, y);
                    var pixelXYY = GetPixel(x, y + 1);

                    double dX = (pixelXXY.R + pixelXXY.G + pixelXXY.B) - (pixelXY.R + pixelXY.G + pixelXY.B);
                    double dY = (pixelXYY.R + pixelXYY.G + pixelXYY.B) - (pixelXY.R + pixelXY.G + pixelXY.B);
                    double gradient = Math.Sqrt(dX*dX + dY*dY);
                    Gradients[x,y] = gradient;

                    if (gradient >= max)
                        max = gradient;
                }
            }

            for (int y = 0; y < Source.PixelHeight - 1; y++)
            {
                for (int x = 0; x < Source.PixelWidth - 1; x++)
                {
                    if (Gradients[x, y] >= GradientLimit)
                    {
                        Edges[x, y] = true;
                        Output.SetPixel(x, y, Colors.White);
                    }
                }
            }
        }
    }
}