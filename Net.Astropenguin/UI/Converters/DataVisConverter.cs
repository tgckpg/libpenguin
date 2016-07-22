using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
    public sealed class DataVisConverter : DataBoolConverter 
    {
        override public object Convert( object value, Type targetType, object parameter, string language )
        {
            return DataBool( value, parameter != null ) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}