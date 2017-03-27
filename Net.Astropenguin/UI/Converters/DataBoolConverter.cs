using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
	public class DataBoolConverter : IValueConverter
	{
		public static readonly string ID = typeof( DataBoolConverter ).Name;

		virtual public object Convert( object value, Type targetType, object parameter, string language )
		{
			return DataBool( value, parameter != null );
		}

		protected bool DataBool( object value, bool Invert = false )
		{
			bool b = false;

			IEnumerable Enu = value as IEnumerable;
			if ( Enu != null )
			{
				b = 0 < Enumerable.Cast<object>( Enu ).Count();
			}
			else if( value is bool )
			{
				b = ( bool ) value;
			}
			else if( value is string )
			{
				b = !string.IsNullOrEmpty( ( string ) value );
			}
			else if( value is int )
			{
				b = ( int ) value != 0;
			}
			else if( value is DateTime )
			{
				b = !( ( DateTime ) value ).Equals( default( DateTime ) );
			}
			else
			{
				b = ( value != null );
			}

			if ( Invert ) return !b;
			return b;
		}

		virtual public object ConvertBack( object value, Type targetType, object parameter, string language )
		{
			return false;
		}
	}
}
