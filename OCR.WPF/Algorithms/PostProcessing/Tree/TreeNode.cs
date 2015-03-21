using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OCR.WPF.Algorithms.PostProcessing.Tree
{
    public class TreeNode
    {
        private readonly Dictionary<int, TreeNode> m_children = new Dictionary<int, TreeNode>();

        public TreeNode(WordTree tree, DictionaryWord label)
        {
            Tree = tree;
            Label = label;
        }

        public WordTree Tree
        {
            get;
            private set;
        }

        public DictionaryWord Label
        {
            get;
            private set;
        }

        public ReadOnlyDictionary<int, TreeNode> Children
        {
            get
            {
                return new ReadOnlyDictionary<int, TreeNode>(m_children);
            }
        }

        public void AddChild(DictionaryWord word)
        {
            int distance = Tree.TreeBuildingWordComparer.Compare(word.Word, Label.Word);
            TreeNode child;
            if (m_children.TryGetValue(distance, out child))
            {
                child.AddChild(word);
            }
            else
            {
                m_children.Add(distance, new TreeNode(Tree, word));
            }
        }
    }
}