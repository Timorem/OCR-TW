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

        public BitmapSource Original { get; set; }

        public Projection VerticalProjection
        {
            get;
            private set;
        }

        public Projection HorizontalProjection
        {
            get;
            private set;
        }

        public CharacterIsolation CharacterIsolation
        {
            get;
            private set;
        }
        public EdgeDetector EdgeDetector
        {
            get;
            private set;
        }


        public void OpenPicture(string file)
        {
            if (!File.Exists(file))
            {
                MessageService.ShowError(null, string.Format("Cannot open file : File {0} doesn't exist", file));
                return;
            }

            Original = new BitmapImage(new Uri(file));

            VerticalProjection = new Projection(Original) {Type = ProjectionType.Vertical};
            VerticalProjection.Compute();

            HorizontalProjection = new Projection(Original) {Type = ProjectionType.Horizontal};
            HorizontalProjection.Compute();

            CharacterIsolation = new CharacterIsolation(Original);
            EdgeDetector = new EdgeDetector(Original);
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

            EdgeDetector.GradientLimit = 20d;
            EdgeDetector.Compute();
            
        }

        #endregion


        #region ApplyCharacterIsolationCommand

        private DelegateCommand m_applyCharacterIsolationCommand;

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

            CharacterIsolation.EdgeDetector.GradientLimit = 20;
            CharacterIsolation.Compute();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}