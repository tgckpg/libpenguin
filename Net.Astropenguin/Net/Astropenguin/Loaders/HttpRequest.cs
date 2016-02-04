using System;
using System.IO;
using System.Text;
using System.Net;

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

        public WebHeaderCollection RequestHeaders
        {
            get { return WCRequest.Headers; }
        }

        public int Timeout
        {
            get { return WCRequest.ContinueTimeout; }
            set { WCRequest.ContinueTimeout = value; }
        }

		protected HttpWebRequest WCRequest;
		protected byte[] PostData;

		public Uri ReqUri;

		public string ContentType
		{
			set { WCRequest.ContentType = value; }
			get { return WCRequest.ContentType; }
		}

		public long ContentLength
		{
			get { return int.Parse( WCRequest.Headers[ HttpRequestHeader.ContentLength ] ); }
		}

        public string Method
        {
            get { return WCRequest.Method; }
            set { WCRequest.Method = value; }
        }

		public HttpRequest( Uri RequestUri )
		{
			ReqUri = RequestUri;
			PostData = new byte[0];
			CreateRequest();
		}

		virtual protected void CreateRequest()
		{
			WCRequest = ( HttpWebRequest ) WebRequest.Create( ReqUri );
			WCRequest.Method = "GET";
			WCRequest.Headers[ HttpRequestHeader.UserAgent ] = "libpenguin - HTTPRequest";
		}

		public void OpenWriteAsync( string DataString )
		{
			// Prepare streaming bytes
			PostData = Encoding.UTF8.GetBytes( DataString );

			OpenWriteAsync();
		}

		public void OpenWriteAsync()
		{
			WCRequest.Headers[ HttpRequestHeader.ContentLength ] = PostData.Length.ToString();
			WCRequest.BeginGetRequestStream( new AsyncCallback( GetRequestStreamCallback ), WCRequest );
		}

        public void OpenAsync()
        {
            WCRequest.BeginGetResponse( new AsyncCallback( GetResponseCallback ), WCRequest );
        }

        public void UpdateProto( Uri Url )
        {
            ReqUri = Url;
            CreateRequest();
        }

		public void OpenAsyncThread( string DataString, bool EnableUIThread )
		{
			// Continuous call and preserve contentType
			string CType = ContentType;
			CreateRequest();
			WCRequest.ContentType = CType;
			EN_UITHREAD = EnableUIThread;
			OpenWriteAsync( DataString );
		}

		public void Stop()
		{
			WCRequest.Abort();
		}

		private void GetRequestStreamCallback( IAsyncResult AsyncResult )
		{
			try
			{
				HttpWebRequest Request = ( HttpWebRequest ) AsyncResult.AsyncState;

				// End Request
				Stream POSTWriter = Request.EndGetRequestStream( AsyncResult );

				// Write request stream.
				POSTWriter.Write( PostData, 0, PostData.Length );
				POSTWriter.Dispose();

				// GetResponse
				Request.BeginGetResponse( new AsyncCallback( GetResponseCallback ) , Request );
			}
			catch ( Exception )
			{
				// MessageBus.Send( typeof( this ), ex.ToString() );
			}
		}

		private void GetResponseCallback( IAsyncResult AsyncResult )
		{
			HttpWebRequest Request = ( HttpWebRequest ) AsyncResult.AsyncState;
			string RefUrl = 0 < PostData.Length
				// Mostly PostData
				? Encoding.UTF8.GetString( PostData, 0, PostData.Length )
				// Rarely GET Requests
				: ReqUri.ToString()
				;
			try
			{
				HttpWebResponse Response = ( HttpWebResponse ) Request.EndGetResponse( AsyncResult );
				StatusCode = Response.StatusCode;

				if ( DRequestCompleted != null )
				{
					byte[] rBytes;
					using ( Stream ResponseStream = Response.GetResponseStream() )
					{
						// Read stream in to byte
						byte[] buffer = new byte[16 * 1024];

						using ( MemoryStream ms = new MemoryStream() )
						{
							int read;
							while ( 0 < ( read = ResponseStream.Read( buffer, 0, buffer.Length ) ) )
							{
								ms.Write( buffer, 0, read );
							}
							rBytes = ms.ToArray();
						}
					}
					DRequestCompletedEventArgs RArgs
						= new DRequestCompletedEventArgs( Response.Headers, RefUrl, rBytes );
					if ( EN_UITHREAD )
						// Raise event in the Main UI thread
						Worker.UIInvoke( () => DRequestCompleted( RArgs ) );
					else
						DRequestCompleted( RArgs );
				}

				// Close HttpWebResponse
				Response.Dispose();
			}
			catch ( Exception ex )
			{
                if ( EN_UITHREAD )
                {
                    Worker.UIInvoke( () =>
                    {
                        // Throw Exception to CompletedArgs
                        DRequestCompleted( new DRequestCompletedEventArgs( RefUrl, ex ) );
                    } );
                }
                else
                {
                    DRequestCompleted( new DRequestCompletedEventArgs( RefUrl, ex ) );
                }
            }
		}

	}
}
