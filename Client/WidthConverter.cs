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
		private double _count;
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var result = (double)values[0]/100 * (double)values[1];
			_count += result;
			Console.WriteLine($"{values[0]} {values[1]} {result} : {_count}");
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
				values[2] is DateTime firstDate &&
				values[3] is DateTime lastDate)
			{
				var total = (lastDate - firstDate).TotalSeconds;
				var untilActivity = (dateTime - firstDate).TotalSeconds;
				var result = (untilActivity / total) * containerWidth;
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
