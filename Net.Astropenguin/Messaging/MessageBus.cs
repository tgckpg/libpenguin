using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Net.Astropenguin.Messaging
{
	public class MessageBus
	{
		public delegate void MessageEvent( Message Mesg );

		private static _Messenger Messenger = new _Messenger();

		public static void Subscribe( object target, MessageEvent e ) => Messenger.AddHandler( target, e );
		public static void Unsubscribe( object target, MessageEvent e ) => Messenger.RemoveHandler( e );

		public static void SendUI( Type Id, string Content, object Payload = null )
		{
			SendUI( new Message( Id, Content, Payload ) );
		}

		public static void SendUI( Message Mesg )
		{
			Helpers.Worker.UIInvoke( () => Messenger.Deliver( Mesg ) );
		}

		public static void Send( Type Id, string Content, object Payload = null )
		{
			Send( new Message( Id, Content, Payload ) );
		}

		public static void Send( Message Mesg )
		{
			Task.Run( () => Messenger.Deliver( Mesg ) );
		}

		private class _Messenger
		{
			public ConcurrentDictionary<MethodInfo, WeakReference<object>> Handlers;

			public _Messenger()
			{
				Handlers = new ConcurrentDictionary<MethodInfo, WeakReference<object>>();
			}

			public void AddHandler( object Target, MessageEvent Handler )
			{
				Handlers[ Handler.GetMethodInfo() ] = new WeakReference<object>( Target );
			}

			public void RemoveHandler( MessageEvent Handler )
			{
				Handlers.TryRemove( Handler.GetMethodInfo(), out WeakReference<object> NOP );
			}

			public void Deliver( Message Mesg )
			{
				foreach ( MethodInfo Handler in Handlers.Keys.ToArray() )
				{
					if ( Handlers.TryGetValue( Handler, out WeakReference<object> WeakEvent ) )
					{
						if ( WeakEvent.TryGetTarget( out object Target ) )
						{
							Handler.Invoke( Target, new object[] { Mesg } );
						}
						else
						{
							Handlers.TryRemove( Handler, out WeakReference<object> NOP );
						}
					}
				}
			}
		}

	}
}