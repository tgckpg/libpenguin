using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
	public sealed class DataCountConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, string language )
		{
			IEnumerable Enu = value as IEnumerable;
			if ( Enu != null ) return Enumerable.Cast<object>( Enu ).Count();

			return 0;
		}

		public object ConvertBack( object value, Type targetType, object parameter, string language )
		{
			return false;
		}
	}
}
