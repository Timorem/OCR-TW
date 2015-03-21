using System.Collections.Generic;
using System.Linq;
using OCR.WPF.Algorithms.PostProcessing.Tree;

namespace OCR.WPF.Algorithms.PostProcessing.Resolver
{
    public class FrequencyResolver : IAmbiguityResolver<DictionaryWord>
    {
        public DictionaryWord FindBetter(string word, IEnumerable<DictionaryWord> collection)
        {
            return collection.OrderByDescending(x => x.Frequency).First();
        }
    }
}