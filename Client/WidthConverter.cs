using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client
{
	internal class WidthConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var result = (double)values[0]/100 * ((double)values[1]);
            return result;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public class DateTimeToPositionConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] is DateTime dateTime &&
				values[1] is double containerWidth &&
				values[2] is double totalDuration &&
				values[3] is DateTime firstDate &&
				values[4] is double lastDateWidth &&
				values[5] is TimeSpan duration)
			{
				var untilActivity = (dateTime - firstDate).TotalSeconds - duration.TotalSeconds;
				var percentage = untilActivity * (containerWidth - lastDateWidth);
                var result = percentage / totalDuration;
				return result;
			}
			return 0;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
