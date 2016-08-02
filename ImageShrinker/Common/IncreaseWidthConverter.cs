using System;
using System.Globalization;
using System.Windows.Data;

namespace ImageShrinker.Common
{
    public class IncreaseWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double) value + 20;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}