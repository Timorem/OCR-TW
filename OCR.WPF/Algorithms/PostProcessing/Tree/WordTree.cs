using System;
using System.Collections.Generic;
using System.Linq;
using OCR.WPF.Algorithms.PostProcessing.Resolver;

namespace OCR.WPF.Algorithms.PostProcessing.Tree
{
    // représente un arbre de mots
    // chaque noeud représente un mot et il possède des fils qui sont tous à une distance différente du noeud parent
    public class WordTree
    {
        public WordTree(IComparer<string> treeBuildingWordComparer, IComparer<string> closerWordComparer, IAmbiguityResolver<DictionaryWord> ambiguityResolver)
        {
            TreeBuildingWordComparer = treeBuildingWordComparer;
            CloserWordComparer = closerWordComparer;
            AmbiguityResolver = ambiguityResolver;
            DistanceThreshold = 1;
        }

        // fonction distance pour créer l'arbre
        public IComparer<string> TreeBuildingWordComparer
        {
            get;
            private set;
        }

        // fonction distance pour chercher le mot le plus proche
        public IComparer<string> CloserWordComparer
        {
            get;
            set;
        }

        // résoud les cas d'ambiguité si plusieurs mots sont a la meme distance
        public IAmbiguityResolver<DictionaryWord> AmbiguityResolver
        {
            get;
            set;
        }

        // racine de l'arbre
        public TreeNode Root
        {
            get;
            private set;
        }
        
        // seuil de distance a partir de laquelle un mot est considéré proche d'un autre
        public int DistanceThreshold
        {
            get;
            set;
        }

        // construit l'arbre
        public void Populate(IEnumerable<DictionaryWord> enumerable)
        {
            foreach (DictionaryWord word in enumerable)
            {
                Populate(word);
            }
        }
        
        // construit l'arbre
        public void Populate(DictionaryWord word)
        {
            if (Root == null)
                Root = new TreeNode(this, word);
            else
            {
                Root.AddChild(word);
            }
        }

        // recherche les mots dans la boule centré sur "word" de rayon "maxDistance"
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

        // trouve le mot le plus proche de word, en resolvant toute ambiguité
        public int FindCloserWord(string word, out string result)
        {
            // trouve les mots à une distance "DistanceThreshold"
            var matching =
                FindCloserNodes(Root, word, DistanceThreshold)
                    .GroupBy(x => CloserWordComparer.Compare(word, x.Label.Word))
                    .OrderBy(x => x.Key).First().ToList();


            // aucune correspondance
            if (matching.Count == 0)
            {
                result = null;
                return -1;
            }

            // resout les ambiguité
            result = AmbiguityResolver.FindBetter(word, matching.Select(x => x.Label)).Word;

            // retourne la distance entre word et result
            return TreeBuildingWordComparer.Compare(word, result);
        }
    }
}