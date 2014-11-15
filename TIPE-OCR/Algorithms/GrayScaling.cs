using System.Drawing;
using System.Drawing.Imaging;

namespace TIPE_OCR.Algorithms
{
    public class GrayScaling
    {
        public static readonly ColorMatrix NTSC_GrayScalingMatrix = new ColorMatrix(
            new[]
            {
                new[] {.3f, .3f, .3f, 0, 0},
                new[] {.59f, .59f, .59f, 0, 0},
                new[] {.11f, .11f, .11f, 0, 0},
                new[] {0f, 0, 0, 1, 0},
                new[] {0f, 0, 0, 0, 1}
            });

        public GrayScaling(Image image)
        {
            Input = image;
        }

        public Image Input { get; set; }

        public Image Output
        {
            get;
            set;
        }

        public void Compute()
        {
            Output = new Bitmap(Input.Width, Input.Height);
            var graphics = Graphics.FromImage(Output);
            var attributes = new ImageAttributes();

            attributes.SetColorMatrix(NTSC_GrayScalingMatrix);
            graphics.DrawImage(Input, new Rectangle(0, 0, Input.Width, Input.Height),
               0, 0, Input.Width, Input.Height, GraphicsUnit.Pixel, attributes);

            graphics.Dispose();
        }
    }
}