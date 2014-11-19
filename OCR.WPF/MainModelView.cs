using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using OCR.WPF.Algorithms;
using OCR.WPF.Properties;
using OCR.WPF.UI;

namespace OCR.WPF
{
    public class MainModelView : INotifyPropertyChanged
    {
        public MainModelView()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public BitmapSource Original
        {
            get { return m_original; }
            set { m_original = value;
                Image = value;
            }
        }

        public BitmapSource Image
        {
            get { return m_image; }
            set { m_image = value;
                OnPropertyChanged();
            }
        }

        public void OpenPicture(string file)
        {
            if (!File.Exists(file))
            {
                MessageService.ShowError(null, string.Format("Cannot open file : File {0} doesn't exist", file));
                return;
            }

            Original = new BitmapImage(new Uri(file));
        }


        #region ApplyEdgeDetectionCommand

        private DelegateCommand m_applyEdgeDetectionCommand;
        private BitmapSource m_image;

        public DelegateCommand ApplyEdgeDetectionCommand
        {
            get { return m_applyEdgeDetectionCommand ?? (m_applyEdgeDetectionCommand = new DelegateCommand(OnApplyEdgeDetection, CanApplyEdgeDetection)); }
        }

        private bool CanApplyEdgeDetection(object parameter)
        {
            return true;
        }

        private void OnApplyEdgeDetection(object parameter)
        {
            if (!CanApplyEdgeDetection(parameter))
                return;

            var alg = new EdgeDetector(Original);
            alg.GradientLimit = 20d;
            alg.Compute();
            Image = alg.Output;
            
        }

        #endregion


        #region ApplyCharacterIsolationCommand

        private DelegateCommand m_applyCharacterIsolationCommand;
        private BitmapSource m_original;

        public DelegateCommand ApplyCharacterIsolationCommand
        {
            get { return m_applyCharacterIsolationCommand ?? (m_applyCharacterIsolationCommand = new DelegateCommand(OnApplyCharacterIsolation, CanApplyCharacterIsolation)); }
        }

        private bool CanApplyCharacterIsolation(object parameter)
        {
            return true;
        }

        private void OnApplyCharacterIsolation(object parameter)
        {
            if (!CanApplyCharacterIsolation(parameter))
                return;

            var alg = new CharacterIsolation(Original);
            alg.EdgeDetector.GradientLimit = 20;
            alg.Compute();
            Image = alg.Output;
        }

        #endregion
    }
}