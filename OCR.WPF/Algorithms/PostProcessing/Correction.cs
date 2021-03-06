﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using OCR.WPF.Algorithms.PostProcessing.Resolver;
using OCR.WPF.Algorithms.PostProcessing.Tree;

namespace OCR.WPF.Algorithms.PostProcessing
{
    public class Correction : AlgorithmBase
    {
        private ObservableCollection<CorrectedWord> m_correctedWords = new ObservableCollection<CorrectedWord>();  

        public Correction(BitmapSource image, WordTree tree)
            : base(image)
        {
            Tree = tree;
        }

        public WordTree Tree
        {
            get;
            private set;
        }

        public string Text
        {
            get;
            set;
        }

        public ReadOnlyObservableCollection<CorrectedWord> CorrectedWords
        {
            get
            {
                return new ReadOnlyObservableCollection<CorrectedWord>(m_correctedWords);
            }
        }


        protected override unsafe void OnCompute()
        {
            // initialisation
            m_correctedWords.Clear();

            // décompose le texte en lignes
            var lines = Text.Split(new []{"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            foreach (var line in lines)
            {
                // décompose le texte en mots
                IEnumerable<string> words =
                    line.Split(new[] {" ", "\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x);
                foreach (string word in words)
                {
                    // pour chaque mot (sans prendre compte des majuscules/minuscules) on cherche le mot du dictionnaire le plus proche
                    var correctedWord = new CorrectedWord(word.ToLowerInvariant(), index);
                    string output;
                    int distance;
                    if ((distance = Tree.FindCloserWord(correctedWord.Original, out output)) >= 0)
                    {
                        correctedWord.Corrected = output;
                        correctedWord.DistanceFromOriginal = distance;
                    }
                    else
                    {
                        correctedWord.DistanceFromOriginal = -1;
                    }

                    m_correctedWords.Add(correctedWord);
                }
                index++;
            }

            OnPropertyChanged("CorrectedWords");
        }
    }
}