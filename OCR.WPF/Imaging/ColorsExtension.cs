using System.Windows.Media;

namespace OCR.WPF.Imaging
{
    public static class ColorsExtension
    {
        public static double GetBrightness(this Color color)
        {
            double r = color.R/255.0d;
            double g = color.G/255.0d;
            double b = color.B/255.0d;

            double max, min;

            max = r;
            min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min)/2;
        }
    }
}