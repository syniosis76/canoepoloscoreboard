using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Utilities
{
    [ValueConversion(typeof(object), typeof(double))]
    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
         object parameter, CultureInfo culture)
        {
            double dblValue = (double)value;
            double scale = Double.Parse(((string)parameter), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            return Math.Max(dblValue * scale, 1);
        }

        public object ConvertBack(object value, Type targetType,
         object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
