using System;
using System.IO;
using System.Text;

using System.Net.Sockets;
using System.Threading;
using System.Net;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.UnitTest
{
	public abstract class NetTrigger : TestExecutor
	{
        public static new readonly string ID = typeof( NetTrigger ).Name;

		ManualResetEvent clientDone = new ManualResetEvent( false );

		public NetTrigger()
		{
#if DEBUG
			Logger.Log( ID, "Checking for open UnitTest protocol", LogType.DEBUG );
			TestTrigger();
#endif
		}

		private void TestTrigger()
		{
			SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

			// NOTE: This is a fake end point. 
			// You must change this to a valid URL or you will get an exception below.

			// Create a socket and connect to the server
			Socket sock = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

			socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>( SocketEventArg_Completed );

			// TCP 9730
			socketEventArg.RemoteEndPoint = new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), 9730 );
			socketEventArg.UserToken = sock;
			sock.ConnectAsync( socketEventArg );
			clientDone.WaitOne();
		}

		// A single callback is used for all socket operations. 
		// This method forwards execution on to the correct handler 
		// based on the type of completed operation
		private void SocketEventArg_Completed( object sender, SocketAsyncEventArgs e )
		{
			switch ( e.LastOperation )
			{
				case SocketAsyncOperation.Connect:
					ProcessConnect( e );
					break;

				case SocketAsyncOperation.Receive:
					ProcessReceive( e );
					break;

				case SocketAsyncOperation.Send:
					ProcessSend( e );
					break;

				default:
					throw new Exception( "Invalid operation completed" );
			}
		}

		// Called when a ConnectAsync operation completes
		private void ProcessConnect( SocketAsyncEventArgs e )
		{
			if ( e.SocketError == SocketError.Success )
			{
				// Successfully connected to the server
				byte[] buffer = Encoding.UTF8.GetBytes( "CanITest" );
				e.SetBuffer( buffer, 0, buffer.Length );
				Socket sock = e.UserToken as Socket;
				bool willRaiseEvent = sock.SendAsync( e );

				if ( !willRaiseEvent )
				{
					ProcessSend( e );
				}
			}
			else
			{
				Logger.Log( ID, "No protocol present", LogType.DEBUG );
                clientDone.Set();
			}
		}

		// Called when a ReceiveAsync operation completes
		// </summary>
		private void ProcessReceive( SocketAsyncEventArgs e )
		{
			if ( e.SocketError == SocketError.Success )
			{
				// Received data from server

				// Data has now been sent and received from the server. 
				// Disconnect from the server
				Socket sock = e.UserToken as Socket;
				string s = Encoding.UTF8.GetString( e.Buffer );
				Logger.Log( ID, "Received a response: " + s, LogType.DEBUG );
				sock.Shutdown( SocketShutdown.Send );
				sock.Dispose();
				clientDone.Set();
				if ( s == "GoAhead" )
				{
					Logger.Log( ID, "Begin UnitTest", LogType.DEBUG );
					Run();
				}
			}
			else
			{
				Logger.Log( ID, "Cannot receive signal from protocol", LogType.DEBUG );
                clientDone.Set();
			}
		}


		// Called when a SendAsync operation completes
		private void ProcessSend( SocketAsyncEventArgs e )
		{
			if ( e.SocketError == SocketError.Success )
			{
				// Sent "Hello World" to the server successfully

				//Read data sent from the server
				Socket sock = e.UserToken as Socket;
				byte[] buffer = new byte[ 7 ];
				e.SetBuffer( buffer, 0, 7 );
				bool willRaiseEvent = sock.ReceiveAsync( e );

				if ( !willRaiseEvent )
				{
					ProcessReceive( e );
				}
			}
			else
			{
				throw new SocketException( ( int ) e.SocketError );
			}
		}
	}
}
