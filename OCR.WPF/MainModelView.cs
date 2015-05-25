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
using Microsoft.Win32;
using OCR.WPF.Algorithms;
using OCR.WPF.Algorithms.PostProcessing;
using OCR.WPF.Algorithms.PostProcessing.Distance;
using OCR.WPF.Algorithms.PostProcessing.Resolver;
using OCR.WPF.Algorithms.PostProcessing.Tree;
using OCR.WPF.Algorithms.PreProcessing;
using OCR.WPF.Algorithms.Processing;
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

        public CharacterRecognition CharacterRecognition
        {
            get;
            private set;
        }

        public Correction Correction
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
            CharacterRecognition = new CharacterRecognition(Original);

            Correction = new Correction(Original, new WordTree(new HammingDistance(false), 
                new HammingDistance('?', false), new FrequencyResolver()));
        }


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
                    Correction.Text = textBuilder.ToString();
                }

                foreach (var character in word.Characters)
                {
                    CharacterRecognition.CharacterZone = character;
                    CharacterRecognition.Compute();
                    textBuilder.Append(CharacterRecognition.RecognizedCharacter);
                }
                
                textBuilder.Append(" ");
            }

            Correction.Text = textBuilder.ToString();
            CorrectTextCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region LoadDictionaryCommand

        public string DictionaryFilePath
        {
            get;
            set;
        }

        private DelegateCommand m_loadDictionaryCommand;


        public DelegateCommand LoadDictionaryCommand
        {
            get { return m_loadDictionaryCommand ?? (m_loadDictionaryCommand = new DelegateCommand(OnLoadDictionary, CanLoadDictionary)); }
        }

        private bool CanLoadDictionary(object parameter)
        {
            return true;
        }

        private void OnLoadDictionary(object parameter)
        {
            if (!CanLoadDictionary(parameter))
                return;

            var dialog = new OpenFileDialog();
            dialog.Title = "Select dictionary ...";
            dialog.Filter = "Dictionary (*.txt)|*.txt";

            if (dialog.ShowDialog() == true)
            {
                LoadDictionary(dialog.FileName);
            }

            CorrectTextCommand.RaiseCanExecuteChanged();
        }

        public void LoadDictionary(string file)
        {
            DictionaryFilePath = file;

            using (var reader = new StringReader(File.ReadAllText(file)))
            {
                string newLine;
                while ((newLine = reader.ReadLine()) != null)
                {
                    var split = newLine.Split(' ');
                    string word;
                    double frequency;
                    if (split.Length < 2)
                    {
                        word = split[0];
                        frequency = 1;
                    }
                    else
                    {
                        word = split[0];
                        if (!double.TryParse(split[1], NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture, out frequency))
                            frequency = 0;
                    }

                    Correction.Tree.Populate(new DictionaryWord(word, frequency));
                }
            }

        }

        #endregion

        #region CorrectTextCommand

        public bool UseHammingDistance
        {
            get { return m_useHammingDistance; }
            set { m_useHammingDistance = value;
                OnDistanceChanged();
            }
        }

        public bool UseLevenstheinDistance
        {
            get { return m_useLevenstheinDistance; }
            set { m_useLevenstheinDistance = value;
                OnDistanceChanged();
            }
        }

        private void OnDistanceChanged()
        {
            if (UseHammingDistance)
                Correction = new Correction(Original, new WordTree(new HammingDistance(false), 
                    new HammingDistance('?', false), new FrequencyResolver()));
            if (UseLevenstheinDistance)
                Correction = new Correction(Original, new WordTree(new LevenshteinDistance(), 
                    new LevenshteinDistance(), new FrequencyResolver()));

            if (!string.IsNullOrEmpty(DictionaryFilePath))
                LoadDictionary(DictionaryFilePath);

            CorrectTextCommand.RaiseCanExecuteChanged();
        }

        private DelegateCommand m_correctTextCommand;
        private bool m_useLevenstheinDistance;
        private bool m_useHammingDistance;


        public DelegateCommand CorrectTextCommand
        {
            get { return m_correctTextCommand ?? (m_correctTextCommand = new DelegateCommand(OnCorrectText, CanCorrectText)); }
        }

        private bool CanCorrectText(object parameter)
        {
            return Correction != null && Correction.Tree.Root != null && !string.IsNullOrEmpty(Correction.Text);
        }

        private void OnCorrectText(object parameter)
        {
            if (!CanCorrectText(parameter))
                return;

            Correction.Compute();
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