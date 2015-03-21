using System.Collections.Generic;

namespace OCR.WPF.Algorithms.PostProcessing.Resolver
{
    public interface IAmbiguityResolver<T>
    {
        T FindBetter(string word, IEnumerable<T> collection);
    }
}