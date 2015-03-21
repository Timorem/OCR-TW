using System;
using System.Collections.Generic;
using System.Linq;
using OCR.WPF.Algorithms.PostProcessing.Resolver;

namespace OCR.WPF.Algorithms.PostProcessing.Tree
{
    public class WordTree
    {
        public WordTree(IComparer<string> treeBuildingWordComparer, IComparer<string> closerWordComparer, IAmbiguityResolver<DictionaryWord> ambiguityResolver)
        {
            TreeBuildingWordComparer = treeBuildingWordComparer;
            CloserWordComparer = closerWordComparer;
            AmbiguityResolver = ambiguityResolver;
            DistanceThreshold = 1;
        }

        public IComparer<string> TreeBuildingWordComparer
        {
            get;
            private set;
        }

        public IComparer<string> CloserWordComparer
        {
            get;
            set;
        }

        public IAmbiguityResolver<DictionaryWord> AmbiguityResolver
        {
            get;
            set;
        }

        public TreeNode Root
        {
            get;
            private set;
        }

        public int DistanceThreshold
        {
            get;
            set;
        }


        public void Populate(IEnumerable<DictionaryWord> enumerable)
        {
            foreach (DictionaryWord word in enumerable)
            {
                Populate(word);
            }
        }

        public void Populate(DictionaryWord word)
        {
            if (Root == null)
                Root = new TreeNode(this, word);
            else
            {
                Root.AddChild(word);
            }
        }

        private IEnumerable<TreeNode> FindCloserNodes(TreeNode currentNode, string word, int maxDistance)
        {
            int distance = CloserWordComparer.Compare(word.ToLowerInvariant(), currentNode.Label.Word);

            if (distance >= 0 && distance <= maxDistance)
                yield return currentNode;

            foreach (
                TreeNode child in
                    currentNode.Children.Where(x => Math.Abs(x.Key - distance) <= maxDistance).Select(x => x.Value))
                foreach (TreeNode node in FindCloserNodes(child, word, maxDistance))
                {
                    yield return node;
                }
        }

        public int FindCloserWord(string word, out string result)
        {
            var matching =
                FindCloserNodes(Root, word, DistanceThreshold)
                    .GroupBy(x => CloserWordComparer.Compare(word, x.Label.Word))
                    .OrderBy(x => x.Key).First().ToList();


            if (matching.Count == 0)
            {
                result = null;
                return -1;
            }

            result = AmbiguityResolver.FindBetter(word, matching.Select(x => x.Label)).Word;
            return TreeBuildingWordComparer.Compare(word, result);
        }
    }
}