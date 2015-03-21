using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using OCR.WPF.Algorithms.PostProcessing;

namespace OCR.WPF.UI.Converters
{
    public class CorrectWordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<CorrectedWord>))
                return new FlowDocument();

            var doc = new FlowDocument();
            int currentLine = 0;
            var paragraph = new Paragraph();
            doc.Blocks.Add(paragraph);
            var words = (IEnumerable<CorrectedWord>) value;
            foreach (var word in words)
            {
                if (word.LineIndex != currentLine)
                {
                    paragraph.Inlines.Add(new Run("\r\n"));
                    currentLine = word.LineIndex;
                }

                var text = new TextBlock(new Run(!string.IsNullOrEmpty(word.Corrected) ? word.Corrected : word.Original));
                //text.FontSize = 24;
                var decoration = new TextDecoration {Location = TextDecorationLocation.Underline};
                if (word.IsCorrect)
                    decoration.Pen = new Pen(Brushes.Green, 2);
                else if (!string.IsNullOrEmpty(word.Corrected) && word.DistanceFromOriginal > 0)
                    decoration.Pen = new Pen(Brushes.Orange, 2);
                else
                    decoration.Pen = new Pen(Brushes.Red, 2);
                text.TextDecorations = new TextDecorationCollection();
                text.TextDecorations.Add(decoration);

                paragraph.Inlines.Add(text);

                paragraph.Inlines.Add(" ");
            }

            return doc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}