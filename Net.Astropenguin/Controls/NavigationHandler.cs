using System;
using System.Collections.Generic;
using Windows.UI.Core;

namespace Net.Astropenguin.Controls
{
    /// <summary>
    /// Modify the flow of NavigatedBack event
    /// Remeber to unsubscribe on page destruct
    /// </summary>
    public class XBackRequestedEventArgs
    {
        private BackRequestedEventArgs OEvent;

        private bool _handled = false;

        public bool Handled {
            get
            {
                return OEvent == null ? _handled : OEvent.Handled;
            }
            set
            {
                if ( OEvent == null )
                {
                    _handled = value;
                }
                else
                {
                    OEvent.Handled = value;
                }
            }
        }

        public XBackRequestedEventArgs( BackRequestedEventArgs e )
        {
            OEvent = e;
        }
    }

    public class NavigationHandler
    {
        private static List<EventHandler<XBackRequestedEventArgs>> NavigationHandlers = new List<EventHandler<XBackRequestedEventArgs>>();

        public static event EventHandler<XBackRequestedEventArgs> OnNavigatedBack
        {
            add
            {
                OnNavigatedBack -= value;
                NavigationHandlers.Add( value );
            }
            remove { NavigationHandlers.Remove( value ); }
        }

        public static void MasterNavigationHandler( object sender, BackRequestedEventArgs e )
        {
            foreach( EventHandler<XBackRequestedEventArgs> H in NavigationHandlers.ToArray() )
            {
                XBackRequestedEventArgs x = new XBackRequestedEventArgs( e );
                H( sender, x );
                if ( x.Handled ) break;
            }
        }

        public static void InsertHandlerOnNavigatedBack( EventHandler<XBackRequestedEventArgs> H )
        {
            OnNavigatedBack -= H;
            NavigationHandlers.Insert( 0, H );
        }
    }

}