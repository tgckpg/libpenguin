using System;
using System.Text;

using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Net.Astropenguin.Logging
{
	public class NetLog
	{
        public static readonly string ID = typeof( NetLog ).Name;

		#if DEBUG
		public static bool Enabled = true;
        public static string RemoteIP = "10.10.0.118";
		#else
		public static bool Enabled = false;
        public static string RemoteIP = "255.255.255.255";
		#endif
		public static bool Ended { get; private set; }

		private static Socket soc;
		// Signaling object used to notify when an asynchronous operation is completed
		static ManualResetEvent _clientDone = new ManualResetEvent( false );
		// Define a timeout in milliseconds for each asynchronous call. If a response is not received within this
		// timeout period, the call is aborted.
		const int TIMEOUT_MILLISECONDS = 5000;
		// The maximum size of the data buffer to use with the asynchronous socket methods
		const int MAX_BUFFER_SIZE = 2048;

		static Exception CrashedEx;
        static IPAddress IP;

		public static void Initialize()
		{
			if ( Enabled && IPAddress.TryParse( RemoteIP, out IP ) )
			{
				soc = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
				if ( soc != null )
				{
					Logger.OnLog += dMesg;
					Logger.Log( ID, "NetLog Begin At: " + DateTime.Now.ToUniversalTime() , LogType.INFO );
				}
            }
        }

        public static void PostInit()
        {
            if ( soc != null )
            {
                Logger.Log( ID, "Socket is already initialized ... perhaps the root log is available?", LogType.INFO );
                return;
            }
            Logger.Log( ID, "Post init Called", LogType.INFO );
            Initialize();
        }

		public static void FireEndSignal( Exception ex )
		{
			// Dispatch the exit message and fire the exit signal
			Logger.Log( "Exception occured, exiting.", LogType.SYSTEM, Signal.EXIT );
			// Save the exception for later, will re throw it at the end
			CrashedEx = ex;
		}

        static void End()
		{
			Logger.OnLog -= dMesg;
			if ( soc != null ) soc.Shutdown( SocketShutdown.Both );
			Ended = true;

			if ( CrashedEx != null )
			{
				// Re throw the exception
				throw CrashedEx;
			}
			else
			{
				throw new NotImplementedException( "Ended without any exception" );
			}
		}

		protected static void dMesg( LogArgs LArgs )
		{
            /*
			Send( new DnsEndPoint( "2.astropneguin.net", 9730 ), LArgs );
			/*/
            Send( new IPEndPoint( IP, 9730 ), LArgs );
			//*/
		}

		/// <summary>
		/// Send the given data to the server using the established connection
		/// </summary>
		/// <param name="serverName">The name of the server</param>
		/// <param name="portNumber">The number of the port over which to send the data</param>
		/// <param name="data">The data to send to the server</param>
		/// <returns>The result of the Send request</returns>
		protected static void Send( EndPoint Ep, LogArgs LArgs )
		{
			// We are re-using the _socket object that was initialized in the Connect method
			if ( soc != null )
			{
				// Create SocketAsyncEventArgs context object
				SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
				// Set properties on context object
				socketEventArg.RemoteEndPoint = Ep;
				// Inline event handler for the Completed event.
				// Note: This event handler was implemented inline in order to make this method self-contained.
				socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>( delegate( object s, SocketAsyncEventArgs e )
				{
					// Unblock the UI thread
					_clientDone.Set();
					if ( LArgs.sig == Signal.EXIT ) End();
				} );
				// Add the data to be sent into the buffer
				byte[] payload = Encoding.UTF8.GetBytes( LArgs.LogStamp );
				socketEventArg.SetBuffer( payload, 0, payload.Length );
				// Sets the state of the event to nonsignaled, causing threads to block
				_clientDone.Reset();
				// Make an asynchronous Send request over the socket
				soc.SendToAsync( socketEventArg );
				// Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
				// If no response comes back within this time then proceed
				_clientDone.WaitOne( TIMEOUT_MILLISECONDS );
			}
		}
	}
}
