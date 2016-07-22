using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.UI.Converters
{
    public sealed class DataStateConverter :DataBoolConverter 
    {
        override public object Convert( object value, Type targetType, object parameter, string language )
        {
            return DataBool( value, parameter != null ) ? ControlState.Reovia : ControlState.Foreatii;
        }
    }
}
