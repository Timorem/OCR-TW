using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
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

        public CharacterRecognition CharacterRecognition
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

            EdgeDetector = new EdgeDetector(Original);
            EdgeDetector.GradientLimit = 20;
            CharacterIsolation = new CharacterIsolation(Original, EdgeDetector);
            CharacterRecognition = new CharacterRecognition(Original);
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

            EdgeDetector.Compute();
            
        }

        #endregion


        #region ApplyCharacterIsolationCommand

        private DelegateCommand m_applyCharacterIsolationCommand;
        private FontFamily m_selectedFont;

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

            CharacterIsolation.Compute();
        }

        #endregion

        #region FontSelection
        
        public FontFamily SelectedFont
        {
            get { return m_selectedFont; }
            set { m_selectedFont = value;
                Typesfaces = m_selectedFont.GetTypefaces();
                CharacterRecognition.Typeface = Typesfaces.First();
            }
        }

        public ICollection<Typeface> Typesfaces
        {
            get;
            set;
        }

        #endregion


        #region RecognizeCommand

        private DelegateCommand m_recognizeCommand;

        public DelegateCommand RecognizeCommand
        {
            get { return m_recognizeCommand ?? (m_recognizeCommand = new DelegateCommand(OnRecognize, CanRecognize)); }
        }

        private bool CanRecognize(object parameter)
        {
            return parameter is Int32Rect;
        }

        private void OnRecognize(object parameter)
        {
            if (parameter == null || !CanRecognize(parameter))
                return;

            var zone = (Int32Rect)parameter;
            CharacterRecognition.CharacterZone = zone;
            CharacterRecognition.Compute();
        }

        #endregion


        #region RecognizeTextCommand

        private DelegateCommand m_recognizeTextCommand;

        public string RecognizedText
        {
            get;
            set;
        }

        public DelegateCommand RecognizeTextCommand
        {
            get { return m_recognizeTextCommand ?? (m_recognizeTextCommand = new DelegateCommand(OnRecognizeText, CanRecognizeText)); }
        }

        private bool CanRecognizeText(object parameter)
        {
            return true;
        }

        private void OnRecognizeText(object parameter)
        {
            if (!CanRecognizeText(parameter))
                return;

            int i = 0;
            var textBuilder = new StringBuilder();
            foreach (var word in CharacterIsolation.Words)
            {
                if (word.LineIndex != i)
                {
                    i = word.LineIndex;
                    textBuilder.Append("\n");
                }

                foreach (var character in word.Characters)
                {
                    CharacterRecognition.CharacterZone = character;
                    CharacterRecognition.Compute();
                    textBuilder.Append(CharacterRecognition.RecognizedCharacter);
                }
                
                textBuilder.Append(" ");
            }

            RecognizedText = textBuilder.ToString();
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