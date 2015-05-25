using System;
using System.Collections.Generic;

namespace OCR.WPF.Algorithms.PostProcessing.Distance
{
    public class LevenshteinDistance : IComparer<string>
    {
        // applique l'algorithme de levensteinh
        // la distance correspond au nombre de substituions, suppressions, 
        // insertions pour atteindre str1 à partir de str2
        public int Compare(string str1, string str2)
        {
            var length1 = str1.Length;
            var length2 = str2.Length;
            // on utilise une matrice
            var matrix = new int[length1 + 1, length2 + 1];

            if (length1 == 0)
                return length2;

            if (length2 == 0)
                return length1;

            for (var i = 0; i <= length1; i++)
                matrix[i, 0] = i;

            for (var j = 0; j <= length2; j++)
                matrix[0, j] = j;

            for (var i = 1; i <= length1; i++)
            {
                for (var j = 1; j <= length2; j++)
                {
                    var cost = (str2[j - 1] == str1[i - 1]) ? 0 : 1;

                    // le coefficient (i,j) correspond à la distance entre
                    // les i premiers caractères de str1 et les j premiers caractères de str2
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            return matrix[length1, length2];
        }
    }
}