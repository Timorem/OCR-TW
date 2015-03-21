using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OCR.WPF.Algorithms.PreProcessing
{
    public class GrayScaling : AlgorithmBase
    {
        public GrayScaling(BitmapSource image)
            : base(image)
        {
        }

        protected override void OnCompute()
        {
            for (int y = 0; y < Input.Height - 1; y++)
            {
                for (int x = 0; x < Input.Width - 1; x++)
                {
                    var pixel = GetPixel(x, y);
                    var grayPixel = Color.FromArgb(255, (byte) (.3f*pixel.R), (byte) (.59f*pixel.G), (byte) (.11f*pixel.B));

                    Output.SetPixel(x, y, grayPixel);
                }
            }
        }
    }
}