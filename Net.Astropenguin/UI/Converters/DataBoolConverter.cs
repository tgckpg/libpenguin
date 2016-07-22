using System;
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

            IEnumerable<object> Enu = value as IEnumerable<object>;
            if ( Enu != null && 0 < Enu.Count() )
            {
                b = true;
            }
            else
            {
                b = ( value as bool? ) == true;
            }

            if ( Invert ) b = !b;

            return b;
        }

        virtual public object ConvertBack( object value, Type targetType, object parameter, string language )
        {
            return false;
        }
    }
}
