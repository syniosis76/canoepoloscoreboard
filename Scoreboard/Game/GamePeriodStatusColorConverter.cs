using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Scoreboard
{
    [ValueConversion(typeof(GamePeriodStatus), typeof(SolidColorBrush))]
    public class GamePeriodStatusColorConverter : IValueConverter
    {
        public static SolidColorBrush ActiveBrush = new SolidColorBrush(Colors.White);
        public static SolidColorBrush InactiveBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x66, 0x66));
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GamePeriodStatus status = (GamePeriodStatus)value;
            return status == GamePeriodStatus.Active ? ActiveBrush : InactiveBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            return DependencyProperty.UnsetValue;
        }
    }
}
