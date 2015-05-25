using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OCR.WPF.Imaging;
using OCR.WPF.Properties;

namespace OCR.WPF.Algorithms
{
    public unsafe abstract class AlgorithmBase : INotifyPropertyChanged
    {
        protected byte[] m_readBuffer;
        protected byte[] m_writeBuffer;
        protected int m_pixelSize;
        protected int m_stride;
        protected bool m_initialized;
        private BitmapSource m_input;

        public AlgorithmBase(BitmapSource image)
        {
            Input = image;
        }

        public BitmapSource Input
        {
            get { return m_input; }
            set { m_input = value;
                Source = value;
                m_initialized = false;
            }
        }

        protected BitmapSource Source
        {
            get;
            set;
        }

        public WriteableBitmap Output
        {
            get;
            protected set;
        }

        public virtual void Initialize()
        {
            // initialise les mémoires tampons en fonction de la taille et format de l'image

            if (Source.Format.BitsPerPixel != 32)
                throw new Exception(string.Format("Format de pixel invalide : {0}", Input.Format));

            m_pixelSize = Source.Format.BitsPerPixel/8;
            m_stride = Source.PixelWidth*m_pixelSize;

            m_readBuffer = new byte[m_stride*Source.PixelHeight];
            Source.CopyPixels(m_readBuffer, m_stride, 0);

            Output = new WriteableBitmap(Source.PixelWidth, Source.PixelHeight, Source.DpiX, Source.DpiY, Source.Format, Source.Palette);
            m_writeBuffer = new byte[m_stride*Source.PixelHeight];

            m_initialized = true;
        }

        // execute l'algorithme et modifie Output
        public void Compute()
        {
            if (!m_initialized)
                Initialize();

            OnCompute();
        }


        protected abstract void OnCompute();

        public Color GetPixel(int x, int y)
        {
            var i = y*m_stride + (x*m_pixelSize);
            return Color.FromArgb(m_readBuffer[i+3], m_readBuffer[i+2], m_readBuffer[i+1], m_readBuffer[i]);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}