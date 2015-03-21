using System;
using System.Collections.Generic;
using System.Linq;

namespace OCR.WPF.Algorithms.PostProcessing.Distance
{
    public class HammingDistance : IComparer<string>
    {
        // match all characters
        public char? Joker
        {
            get;
            set;
        }

        public bool SameLength
        {
            get;
            set;
        }

        public HammingDistance()
        {
            SameLength = false;
        }

        public HammingDistance(char joker, bool sameLength) : this (sameLength)
        {
            Joker = joker;
        }

        public HammingDistance(bool sameLength)
        {
            SameLength = sameLength;
        }

        public int Compare(string a, string b)
        {
            if (a.Length != b.Length)
                return -1;

            return a.Zip(b, (x, y) => x != y || (Joker.HasValue && x == Joker || y == Joker) ? 1 : 0).Sum() + Math.Abs(a.Length - b.Length);
        }
    }
}