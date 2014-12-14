using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Imaging;

namespace OCR.WPF.Algorithms
{
    public class EdgeDetector : AlgorithmBase
    {
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
            Output.Clear(Colors.Black);
            Gradients = new double[Source.PixelWidth,Source.PixelHeight];
            var derivativeX = new double[Source.PixelWidth,Source.PixelHeight];
            var derivativeY = new double[Source.PixelWidth,Source.PixelHeight];
            var nonMaxGradient = new double[Source.PixelWidth,Source.PixelHeight];
            Edges = new bool[Source.PixelWidth,Source.PixelHeight];

            for (int y = 2; y < Source.PixelHeight - 2; y++)
            {
                for (int x = 2; x < Source.PixelWidth - 2; x++)
                {
                    var pixelXY = GetPixel(x, y);
                    var pixelXXY = GetPixel(x + 1, y);
                    var pixelXYY = GetPixel(x, y + 1);
                    derivativeX[x,y] = ((pixelXXY.R + pixelXXY.G + pixelXXY.B) - (pixelXY.R + pixelXY.G + pixelXY.B))/3d;
                    derivativeY[x,y] = ((pixelXYY.R + pixelXYY.G + pixelXYY.B) - (pixelXY.R + pixelXY.G + pixelXY.B))/3d;
                    Gradients[x, y] = derivativeX[x, y]*derivativeX[x, y] + derivativeY[x, y]*derivativeY[x, y];
                    nonMaxGradient[x, y] = Gradients[x, y];
                }
            }

            for (int y = 2; y < Source.PixelHeight - 2; y++)
            {
                for (int x = 2; x < Source.PixelWidth - 2; x++)
                {
                    var theta = Math.Abs(derivativeX[x, y]) < 0.01 ? Math.PI/2 : Math.Abs(Math.Atan((double)derivativeY[x, y]/derivativeX[x, y]));

                    // horizontal
                    if (theta <= Math.PI/8 || theta >= 7*Math.PI/8)
                    {
                        if (Gradients[x, y] <= Gradients[x + 1, y] || Gradients[x, y] <= Gradients[x - 1, y])
                            nonMaxGradient[x, y] = 0;
                    }
                    // +45 degree
                    else if (theta >= 3*Math.PI/8)
                    {
                        if (Gradients[x, y] <= Gradients[x + 1, y + 1] || Gradients[x, y] <= Gradients[x - 1, y - 1])
                            nonMaxGradient[x, y] = 0;
                    }
                    // vertical
                    else if (theta >= 5*Math.PI/8)
                    {
                        if (Gradients[x, y] <= Gradients[x - 1, y + 1] || Gradients[x, y] <= Gradients[x + 1, y - 1])
                            nonMaxGradient[x, y] = 0;
                    }
                    // -45 degree
                    else if (theta >= 7*Math.PI/8)
                        if (Gradients[x, y] <= Gradients[x, y + 1] || Gradients[x, y] <= Gradients[x, y - 1])
                            nonMaxGradient[x, y] = 0;
                }
            }

            for (int y = 2; y < Source.PixelHeight - 2; y++)
            {
                for (int x = 2; x < Source.PixelWidth - 2; x++)
                {
                    Gradients[x,y] = Math.Sqrt(Gradients[x, y]);
                    if (Gradients[x, y] > GradientLimit)
                    {
                        Edges[x, y] = true;
                        Output.SetPixel(x, y, Colors.White);
                    }
                }
            }
        }
    }
}