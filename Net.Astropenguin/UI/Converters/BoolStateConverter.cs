using System;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
    public class BoolStateConverter : IValueConverter
    {
        public static readonly string ID = typeof( BoolStateConverter ).Name;

        public object Convert( object value, Type targetType, object parameter, string language )
        {
            bool b = ( bool ) value;
            if ( parameter != null ) b = !b;
            return b ? ControlState.Reovia : ControlState.Foreatii;
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language )
        {
            return false;
        }
    }
}
