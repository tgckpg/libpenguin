using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
    public sealed class EnumNameConvorter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, string language )
        {
            return Enum.GetName( value.GetType(), value );
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language )
        {
            throw new NotImplementedException();
        }
    }
}
