using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace OCR.WPF.UI.Converters
{
    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(a => a == DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;


            var x = (int)values[0];
            var y = (int)values[1];

            return x*y;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}