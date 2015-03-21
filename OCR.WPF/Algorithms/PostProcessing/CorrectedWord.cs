namespace OCR.WPF.Algorithms.PostProcessing
{
    public class CorrectedWord
    {
        public CorrectedWord(string original, int lineIndex)
        {
            Original = original;
            LineIndex = lineIndex;
        }

        public string Original
        {
            get;
            set;
        }

        public string Corrected
        {
            get;
            set;
        }

        public bool IsCorrect
        {
            get
            {
                return DistanceFromOriginal == 0 && !string.IsNullOrEmpty(Corrected);
            }
        }

        public int DistanceFromOriginal
        {
            get;
            set;
        }

        public int LineIndex
        {
            get;
            set;
        }
    }
}