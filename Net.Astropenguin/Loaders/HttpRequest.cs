using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Windows.Foundation;

using Net.Astropenguin.Helpers;
using Net.Astropenguin.Logging;

namespace Net.Astropenguin.Loaders
{
	public class HttpRequest
	{
		public static readonly string ID = typeof( HttpRequest ).Name;

		#region DThread Handlers
		public delegate void DRequestCompleteHandler( DRequestCompletedEventArgs DArgs );

		private event DRequestCompleteHandler DRequestCompleted;

		public event DRequestCompleteHandler OnRequestComplete
		{
			add
			{
				DRequestCompleted -= value;
				DRequestCompleted += value;
			}
			remove
			{
				DRequestCompleted -= value;
			}
		}
		#endregion

		public HttpStatusCode StatusCode;
		public bool EN_UITHREAD = true;

		public HttpRequestHeaders RequestHeaders
		{
			get { return WCMessage.Headers; }
		}

		public int Timeout { get; set; }

		protected HttpClient WCRequest;
		protected HttpRequestMessage WCMessage;
		protected HttpClientHandler ClientHandler;

		protected CookieContainer Kookies
		{
			get { return ClientHandler.CookieContainer; }
			set { ClientHandler.CookieContainer = value; }
		}

		protected byte[] PostData;
		private IAsyncOperation<HttpResponseMessage> AsyncOp;

		public Uri ReqUri;

		public string ContentType { get; set; }
		public string UserAgent { get; protected set; }

		public long ContentLength
		{
			get { return ( long ) WCMessage.Content.Headers.ContentLength; }
		}

		public HttpMethod Method
		{
			get { return WCMessage.Method; }
			set { WCMessage.Method = value; }
		}

		public HttpRequest( Uri RequestUri )
		{
			ReqUri = RequestUri;

			PostData = new byte[0];
			CreateRequest();
		}

		virtual protected void CreateRequest()
		{
			ClientHandler = new HttpClientHandler()
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};

			WCRequest = new HttpClient( ClientHandler );
			WCMessage = new HttpRequestMessage( HttpMethod.Get, ReqUri );
		}

		virtual protected void SetProps()
		{
			if ( !( WCMessage.Content == null || string.IsNullOrEmpty( ContentType ) ) )
			{
				WCMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse( ContentType );
			}

			if ( string.IsNullOrEmpty( UserAgent ) )
			{
				WCMessage.Headers.Add( "User-Agent", "libpenguin - HTTPRequest" );
			}
			else
			{
				WCMessage.Headers.Add( "User-Agent", UserAgent );
			}

			if ( Timeout != 0 )
			{
				WCRequest.Timeout = TimeSpan.FromMilliseconds( Timeout );
			}
		}

		public void OpenWriteAsync( string DataString )
		{
			// Prepare streaming bytes
			PostData = Encoding.UTF8.GetBytes( DataString );
			OpenWriteAsync();
		}

		// Primary Request Method
		public void OpenWriteAsync()
		{
			WCMessage.Content = new ByteArrayContent( PostData );
			SetProps();
			Send();
		}

		// Primary Request Method
		public void OpenAsync()
		{
			SetProps();
			Send();
		}

		public void UpdateProto( Uri Url )
		{
			ReqUri = Url;
			CreateRequest();
		}

		public void OpenAsyncThread( string DataString, bool EnableUIThread )
		{
			CreateRequest();

			EN_UITHREAD = EnableUIThread;
			OpenWriteAsync( DataString );
		}

		public void Stop()
		{
			AsyncOp.Cancel();
		}

		private async void Send()
		{
			try
			{
				AsyncOp = WCRequest.SendAsync( WCMessage ).AsAsyncOperation();
				GetResponseCallback( await AsyncOp );
			}
			catch ( OperationCanceledException ex )
			{
				RequestException( ex );
			}
			catch ( HttpRequestException ex )
			{
				RequestException( ex );
			}
			catch ( Exception )
			{
				// MessageBus.Send( typeof( this ), ex.ToString() );
			}
		}

		private void RequestException( Exception ex )
		{
			string RefUrl = 0 < PostData.Length
				? Encoding.UTF8.GetString( PostData, 0, PostData.Length )
				: ReqUri.ToString()
				;
			RequestComplete( new DRequestCompletedEventArgs( RefUrl, ex ) );
		}

		private async void GetResponseCallback( HttpResponseMessage Response )
		{
			string RefUrl = 0 < PostData.Length
				// Mostly PostData
				? Encoding.UTF8.GetString( PostData, 0, PostData.Length )
				// Rarely GET Requests
				: ReqUri.ToString()
				;
			try
			{
				StatusCode = Response.StatusCode;

				if ( DRequestCompleted != null )
				{
					byte[] rBytes;

					using ( Stream ResponseStream = await Response.Content.ReadAsStreamAsync() )
					{
						ReadResponse( ResponseStream, out rBytes );
					}

					CookieCollection CC = ClientHandler.CookieContainer.GetCookies( ReqUri );
					DRequestCompletedEventArgs RArgs = new DRequestCompletedEventArgs( Response, CC, RefUrl, rBytes );
					RequestComplete( RArgs );
				}

				// Close HttpWebResponse
				Response.Dispose();
			}
			catch ( Exception ex )
			{
				RequestComplete( new DRequestCompletedEventArgs( RefUrl, ex ) );
			}
		}

		private void RequestComplete( DRequestCompletedEventArgs Args )
		{
			// Raise event in the Main UI thread
			if ( EN_UITHREAD ) Worker.UIInvoke( () => DRequestCompleted( Args ) );
			else Worker.Register( () => DRequestCompleted( Args ) );
		}

		private void ReadResponse( Stream s, out byte[] rBytes )
		{
			// Read stream in to byte
			byte[] buffer = new byte[ 16 * 1024 ];

			using ( MemoryStream ms = new MemoryStream() )
			{
				int read;
				while ( 0 < ( read = s.Read( buffer, 0, buffer.Length ) ) )
				{
					ms.Write( buffer, 0, read );
				}
				rBytes = ms.ToArray();
			}
		}

	}
}