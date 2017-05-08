using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Utilities
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class PeriodTimeConverter : IValueConverter    
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTimeValue = (DateTime)value;
            string result = dateTimeValue.ToString("HH:mm");
            if (dateTimeValue.Second != 0)
            {
                result += ":" + dateTimeValue.ToString("ss");
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
