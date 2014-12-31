using System.Windows;
using System.Windows.Media.Imaging;

namespace OCR.WPF.Algorithms
{
    public class ComparaisonEntry
    {
        public ComparaisonEntry(BitmapSource original, Int32Rect zone, char c, byte[] buffer, byte[] differenceBuffer, double trustLevel)
        {
            Character = c;
            var bitmap = new WriteableBitmap(zone.Width, zone.Height, original.DpiX, original.DpiY, original.Format, original.Palette);
            bitmap.WritePixels(new Int32Rect(0,0, zone.Width,zone.Height), buffer, zone.Width*4, 0);
            CharacterBitmap = bitmap;

            bitmap = new WriteableBitmap(zone.Width, zone.Height, original.DpiX, original.DpiY, original.Format, original.Palette);
            bitmap.WritePixels(new Int32Rect(0,0, zone.Width,zone.Height), differenceBuffer, zone.Width*4, 0);
            DifferenceBitmap = bitmap;

            TrustLevel = trustLevel;
        }

        public char Character
        {
            get;
            set;
        }

        public BitmapSource CharacterBitmap
        {
            get;
            set;
        }

        public BitmapSource DifferenceBitmap
        {
            get;
            set;
        }

        public double TrustLevel
        {
            get;
            set;
        }
    }
}