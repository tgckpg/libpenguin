using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
    public class DataVisConverter : IValueConverter
    {
        public static readonly string ID = typeof( DataStateConverter ).Name;

        public object Convert( object value, Type targetType, object parameter, string language )
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

            if ( parameter != null ) b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language )
        {
            return false;
        }
    }
}