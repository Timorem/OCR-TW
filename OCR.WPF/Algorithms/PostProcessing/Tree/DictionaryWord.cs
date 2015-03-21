namespace OCR.WPF.Algorithms.PostProcessing.Tree
{
    public class DictionaryWord
    {
        public DictionaryWord(string word, double frequency)
        {
            Word = word;
            Frequency = frequency;
        }

        public string Word
        {
            get;
            set;
        }

        public double Frequency
        {
            get;
            set;
        }
    }
}