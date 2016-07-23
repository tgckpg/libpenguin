using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.Messaging
{
    public class MessageBus
    {
        public delegate void MessageEvent( Message Mesg );

        private static event MessageEvent DoDelivery;

        public static event MessageEvent OnDelivery
        {
            add
            {
                DoDelivery -= value;
                DoDelivery += value;
            }
            remove
            {
                DoDelivery -= value;
            }
        }

        public static void SendUI( Message Mesg )
        {
            Helpers.Worker.UIInvoke( () => { DoDelivery?.Invoke( Mesg ); } );
        }

        public static void Send( Message Mesg )
        {
            Task.Run( () => { DoDelivery?.Invoke( Mesg ); } );
        }
    }
}