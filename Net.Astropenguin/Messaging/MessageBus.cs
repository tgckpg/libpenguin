using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Net.Astropenguin.Messaging
{
	public delegate void MessageEvent( Message Mesg );

	public class Messenger
	{
		private struct MRef
		{
			public int HKey;
			public MethodInfo Method;
		}

		private ConcurrentDictionary<MRef, WeakReference<object>> Handlers;

		public Messenger()
		{
			Handlers = new ConcurrentDictionary<MRef, WeakReference<object>>();
		}

		public void AddHandler( object Target, MessageEvent Handler )
		{
			MRef Ref = new MRef() { Method = Handler.GetMethodInfo(), HKey = Target.GetHashCode() };
			Handlers[ Ref ] = new WeakReference<object>( Target );
		}

		public void RemoveHandler( object Target, MessageEvent Handler )
		{
			int HKey = Target.GetHashCode();
			MethodInfo HInfo = Handler.GetMethodInfo();
			MRef Ref = Handlers.Keys.FirstOrDefault( x => x.Method == HInfo && x.HKey == HKey );

			if ( !Ref.Equals( default( MRef ) ) )
			{
				Handlers.TryRemove( Ref, out WeakReference<object> NOP );
			}
		}

		public void Deliver( Message Mesg )
		{
			foreach ( MRef Ref in Handlers.Keys.ToArray() )
			{
				if ( Handlers.TryGetValue( Ref, out WeakReference<object> WeakEvent ) )
				{
					if ( WeakEvent.TryGetTarget( out object Target ) )
					{
						Ref.Method.Invoke( Target, new object[] { Mesg } );
					}
					else
					{
						Handlers.TryRemove( Ref, out WeakReference<object> NOP );
					}
				}
			}
		}
	}

	public class MessageBus
	{
		private static Messenger Messenger = new Messenger();

		public static void Subscribe( object target, MessageEvent e ) => Messenger.AddHandler( target, e );
		public static void Unsubscribe( object target, MessageEvent e ) => Messenger.RemoveHandler( target, e );

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
	}

}