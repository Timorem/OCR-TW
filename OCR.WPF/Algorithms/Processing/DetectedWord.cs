using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace OCR.WPF.Algorithms.Processing
{
    public class DetectedWord : INotifyPropertyChanged
    {
        public DetectedWord(Int32Rect region, int lineIndex)
        {
            LineIndex = lineIndex;
            Region = region;
            Characters = new ObservableCollection<Int32Rect>();
        }

        public Int32Rect Region
        {
            get;
            set;
        }

        public int LineIndex
        {
            get;
            set;
        }

        public ObservableCollection<Int32Rect> Characters
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}