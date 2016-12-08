using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.Loaders
{
	public class DThreadArgs : EventArgs
	{
		public string FileLocation { get; private set; }
		public Guid Id { get; private set; }
		public DThreadArgs( string SaveLocation, Guid id )
		{
			FileLocation = SaveLocation;
			Id = id;
		}
	}

	public class DThreadUpdateArgs : DThreadArgs
	{
		public string Desc { get; private set; }
		public DThreadUpdateArgs( string SaveLocation, Guid id, string desc )
			: base( SaveLocation, id )
		{
			Desc = desc;
		}
	}

	public class DTheradCompleteArgs : DThreadArgs
	{
		public DTheradCompleteArgs( string SaveLocation, Guid id )
			: base( SaveLocation, id )
		{
		}
	}

	public class DCycleCompleteArgs : EventArgs 
	{
		public DCycleCompleteArgs()
			: base()
		{
		}
	}

	public class DRequestCompletedEventArgs : EventArgs
	{
		public static readonly string ID = typeof( DRequestCompletedEventArgs ).Name;

		private string rString;
		private Exception RequestException;
		private byte[] RBytes;

		public string RequestUrl { get; private set; }

		public Exception Error { get; private set; }

		public HttpResponseHeaders ResponseHeaders { get; private set; }

		public byte[] ResponseBytes
		{
			get
			{
				// Throw exception if exception exists
				if ( RequestException != null ) throw RequestException;
				return RBytes;
			}
		}

		public string ResponseString
		{
			get
			{
				// Throw exception if exception exists
				if ( RequestException != null ) throw RequestException;
				if ( rString == null )
				{
					rString = Encoding.UTF8.GetString( RBytes, 0, RBytes.Length );
					if ( !string.IsNullOrEmpty( rString ) && rString[0] == 65279 )
						// Remove EF BB BF
						rString = rString.Substring( 1 );
				}
				return rString;
			}
		}

		public CookieCollection Cookies { get; private set; }

		public DRequestCompletedEventArgs( HttpResponseMessage Resp, CookieCollection Cookies, string Url, byte[] RawBytes )
		{
			RequestUrl = Url;
			RBytes = RawBytes;
			ResponseHeaders = Resp.Headers;

            this.Cookies = Cookies;
		}

		public DRequestCompletedEventArgs( string Url, Exception ex )
		{
			// Failed Exception will thrown in Getting ResponseString or ResponseBytes Method
			Logger.Log( ID, "A WebException occured at " + Url + ": " + ex.Message, LogType.DEBUG );

			RequestUrl = Url;
			RequestException = ex;
		}
	}

	public class DResponseSavedEventArgs : EventArgs
	{
		public string SaveLocation { get; private set; }
		public DResponseSavedEventArgs( string SLocation )
		{
			SaveLocation = SLocation;
		}
	}

	public class DThreadProgressArgs : DThreadArgs
	{
		public long BytesReceived { get; private set; }
		public long BytesTotal { get; private set; }
		public int Percentage
		{
			get
			{
				return Convert.ToInt32( ( Convert.ToDouble( BytesReceived ) / Convert.ToDouble( BytesTotal ) * 100 ) );
			}
		}
		public DThreadProgressArgs( string SaveLocation, long TotalBytesReceived, long TotalBytesToReceive, Guid id )
			: base( SaveLocation, id )
		{
			BytesReceived = TotalBytesReceived;
			BytesTotal = TotalBytesToReceive;
			if ( BytesTotal == 0 )
			{
				BytesTotal = 1;
				BytesReceived = 0;
			}
		}
	}
}