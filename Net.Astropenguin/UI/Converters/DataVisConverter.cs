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

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IEnumerable<object> Enu = value as IEnumerable<object>;

            bool b = value != null;

            if( Enu != null && Enu.Count() == 0 )
            {
                b = false;
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