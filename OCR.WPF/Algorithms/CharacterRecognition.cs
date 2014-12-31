using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Imaging;

namespace OCR.WPF.Algorithms
{
    public class CharacterRecognition : AlgorithmBase
    {
        
        private static char[] PRINTEABLE_CHARS =
            new char[]
            {
                //'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };


        public CharacterRecognition(BitmapSource image)
            : base(image)
        {
            Entries = new ObservableCollection<ComparaisonEntry>();
        }

        public Int32Rect CharacterZone
        {
            get;
            set;
        }

        public Typeface Typeface
        {
            get;
            set;
        }

        public double TrustLevel
        {
            get;
            set;
        }

        public char RecognizedCharacter
        {
            get;
            set;
        }

        public ObservableCollection<ComparaisonEntry> Entries
        {
            get;
            set;
        }

        protected override void OnCompute()
        {
            Entries.Clear();
            ComparaisonEntry max = null;
            double maxLevel = 0;

            var inputProportions = (double)CharacterZone.Width/CharacterZone.Height;
            foreach (var entry in EnumerateCharactersMap(Typeface, CharacterZone.Width, CharacterZone.Height))
            {
                byte[] difference = new byte[entry.BitmapBuffer.Length];

                int sum = 0;
                int sumTotal = 0;
                for (int x = 0; x < CharacterZone.Width; x++)
                {
                    for (int y = 0; y < CharacterZone.Height; y++)
                    {
                        var pixelA = GetPixel(CharacterZone.X + x, CharacterZone.Y + y);
                        var pixelB = GetPixel(entry.BitmapBuffer, x, y, CharacterZone.Width, 4);

                        // is not white
                        if (pixelA.GetBrightness() < 0.99)
                        {
                            if (pixelB.GetBrightness() < 0.99)
                                sum++;
                            sumTotal++;
                        }

                        SetPixel(difference, x, y, pixelA - pixelB, CharacterZone.Width, 4);
                    }
                }

                var ratio = sum/(double)sumTotal;
                var diff = 1 - Math.Abs(inputProportions - entry.Proportion);
                var compEntry = new ComparaisonEntry(Source, CharacterZone, entry.Character, entry.BitmapBuffer, difference, diff*ratio);
                Entries.Add(compEntry);
                if (compEntry.TrustLevel > maxLevel)
                {
                    max = compEntry;
                    maxLevel = compEntry.TrustLevel;
                }
            }

            TrustLevel = max.TrustLevel;
            if (TrustLevel > 0.1)
                RecognizedCharacter = max.Character;
            else
                RecognizedCharacter = '?';
        }

        private static Color GetPixel(byte[] buffer, int x, int y, int width, int pixelSize)
        {
            var i = y*width*pixelSize + (x*pixelSize);
            return Color.FromArgb(buffer[i+3], buffer[i+2], buffer[i+1], buffer[i]);
        }
        private static void SetPixel(byte[] buffer, int x, int y, Color color, int width, int pixelSize)
        {
            var i = y*width*pixelSize + (x*pixelSize);
            buffer[i + 3] = color.A;
            buffer[i + 2] = color.R;
            buffer[i + 1] = color.G;
            buffer[i] = color.B;
        }

        private static IEnumerable<CharacterMapEntry> EnumerateCharactersMap(Typeface typeface, int width, int height)
        {
            foreach (var c in PRINTEABLE_CHARS)
            {
                double proportion;
                var visual = new DrawingVisual();
                using (DrawingContext drawingContext = visual.RenderOpen())
                {
                    var size = height*96.0/72.0;
                    var text = new FormattedText(c.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, size, Brushes.White);
                    var geom = text.BuildGeometry(new Point(0, 0));
                    
                    drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                    var translateTransform = new TranslateTransform();
                    translateTransform.X = -geom.Bounds.Left;
                    translateTransform.Y = -geom.Bounds.Top;
                    proportion = geom.Bounds.Width/geom.Bounds.Height;
                    drawingContext.PushTransform(translateTransform);
                    var scaleTransform = new ScaleTransform(width/geom.Bounds.Width, height/geom.Bounds.Height);
                    drawingContext.PushTransform(scaleTransform);
                    drawingContext.DrawGeometry(Brushes.Black, null, geom);
                    //drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                    //drawingContext.DrawText(text, new Point(0, 0));
                }
                var resizedImage = new RenderTargetBitmap(
                    width, height, // Resized dimensions
                    96, 96, // Default DPI values
                    PixelFormats.Default); // Default pixel format
                resizedImage.Render(visual);

                var stride = 4*width;
                var buffer = new byte[stride*height];
                resizedImage.CopyPixels(buffer, stride, 0);

                yield return new CharacterMapEntry(c, buffer, proportion);
            }
        }
    }
}