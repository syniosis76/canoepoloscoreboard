using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Scoreboard
{    
    [ValueConversion(typeof(TimeSpan), typeof(String))]
    public class GameTimeConverter : IValueConverter
    {
        public static string ToString(TimeSpan timeSpan)
        {                        
            int seconds = timeSpan.Milliseconds > 500 ? timeSpan.Seconds + 1 : timeSpan.Seconds; // Round up seconds for display.
            int minutes = timeSpan.Minutes;            
            int hours = timeSpan.Hours;

            if (seconds == 60)
            {
                seconds = 0;
                minutes += 1;
            }

            if (minutes == 60)
            {
                minutes = 0;
                hours += 1;
            }
            
            if (hours != 0)
            {
                return hours.ToString() + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
            }
            else if (minutes != 0)
            {
                return minutes.ToString() + ":" + seconds.ToString("00");
            }
            else
            {
                return seconds.ToString("00");
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return GameTimeConverter.ToString((TimeSpan)value);
            }
            else
            {
                return null;
            }
        }        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            string stringValue = value as string;
            if (!String.IsNullOrEmpty(stringValue))
            {
                string[] stringValues = stringValue.Split(':');
                        
                int hours = stringValues.Length >= 3 ? int.Parse(stringValues[0]) : 0;
                int minutes = stringValues.Length >= 2 ? int.Parse(stringValues[stringValues.Length - 2]) : 0;
                int seconds = int.Parse(stringValues[stringValues.Length - 1]);

                return new TimeSpan(hours, minutes, seconds);
            }            

            return null;
        }
    }
}
