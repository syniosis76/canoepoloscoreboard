using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Utilities
{
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class BoolToBrushConverter : IValueConverter
	{
		public Brush TrueBrush { get; set; } = new SolidColorBrush(Colors.White);
		public Brush FalseBrush { get; set; } = new SolidColorBrush(Colors.Black);

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)	//throw if non-bool
				return TrueBrush;
			else
				return FalseBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
