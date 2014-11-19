using System.Runtime.InteropServices;

namespace OCR.WPF.Imaging
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel32
    {
        public Pixel32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public byte B;
        public byte G;
        public byte R;
        public byte A;


        public double Luminance
        {
            get
            {
                // 0.2126*R + 0.7152*G + 0.0722*B
                return 0.2126*R + 0.7152*G + 0.0722*B;
            }
        }

        public static Pixel32 FromBrightness(double brightness)
        {
            var pixel = new Pixel32 {A = 255, R = (byte) (255*brightness), G = (byte) (255*brightness), B = (byte) (255*brightness)};
            return pixel;
        }
    }
}