using System;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
    public class DataStateConverter : IValueConverter
    {
        public static readonly string ID = typeof( DataStateConverter ).Name;

        public object Convert( object value, Type targetType, object parameter, string language )
        {
            bool b = false;

            if ( value != null ) b = ( bool ) value;
            if ( parameter != null ) b = !b;

            return b ? ControlState.Reovia : ControlState.Foreatii;
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language )
        {
            return false;
        }
    }
}
