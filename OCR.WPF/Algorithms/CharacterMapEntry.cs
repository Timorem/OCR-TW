namespace OCR.WPF.Algorithms
{
    public class CharacterMapEntry
    {
        public CharacterMapEntry(char character, byte[] bitmapBuffer, double proportion)
        {
            Character = character;
            BitmapBuffer = bitmapBuffer;
            Proportion = proportion;
        }

        public char Character
        {
            get;
            set;
        }

        public byte[] BitmapBuffer
        {
            get;
            set;
        }

        public double Proportion
        {
            get;
            set;
        }
    }
}