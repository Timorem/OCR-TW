using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Imaging;

namespace OCR.WPF.Algorithms.PreProcessing
{
    public enum ProjectionType
    {
        Horizontal,
        Vertical,
        Other
    }


    public class Projection : AlgorithmBase
    {
        public Projection(BitmapSource image) : base(image)
        {
            OutputHeight = 64;
        }

        public ProjectionType Type
        {
            get;
            set;
        }

        public int Degree
        {
            get;
            set;
        }

        public int OutputHeight
        {
            get;
            set;
        }

        public byte[] ProjectionOutput
        {
            get;
            private set;
        }

        public override unsafe void Initialize()
        {
            base.Initialize();

            Output = Type != ProjectionType.Horizontal
                ? new WriteableBitmap(Source.PixelWidth, OutputHeight, Source.DpiX, Source.DpiY, Source.Format,
                    Source.Palette)
                : new WriteableBitmap(OutputHeight, Source.PixelHeight, Source.DpiX, Source.DpiY, Source.Format,
                    Source.Palette);

            Output.Clear(Colors.White);
            m_writeBuffer = new byte[m_pixelSize*Output.PixelHeight*Output.PixelWidth];
        }

        protected override unsafe void OnCompute()
        {
            if (Type != ProjectionType.Other)
            {
                ProjectionOutput = new byte[Type == ProjectionType.Horizontal ? Source.PixelHeight : Source.PixelWidth];

                if(Type == ProjectionType.Vertical)
                    for (int x = 0; x < Source.PixelWidth; x++)
                    {
                        double sum = 0;
                        for (int y = 0; y < Source.PixelHeight; y++)
                        {
                            var pixel = GetPixel(x, y);
                            sum += pixel.GetBrightness();
                        }
                        var value = sum/Source.PixelHeight;
                        ProjectionOutput[x] = (byte)(255*value);
                        Output.DrawLine(x, 0, x, (int)(OutputHeight * (1 - value)), Colors.Black);
                    }
                else
                    for (int x = 0; x < Source.PixelHeight; x++)
                    {
                        double sum = 0;
                        for (int y = 0; y < Source.PixelWidth; y++)
                        {
                            var pixel = GetPixel(y, x);
                            sum += (pixel.R + pixel.G + pixel.A) / (3d*255);
                        }
                        var value = sum / Source.PixelWidth;
                        ProjectionOutput[x] = (byte)(255 * value);
                        Output.DrawLine(0, x, (int)(OutputHeight * (1 - value)), x, Colors.Black);

                    }
            }
        }
    }
}