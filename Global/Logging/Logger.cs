using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Net.Astropenguin.Logging
{
	public class Logger
	{

		public delegate void LogEvent( LogArgs LogArgs );

		private static event LogEvent WLogHandler;

		public static List<LogType> LogFilter = new List<LogType>();

		public static event LogEvent OnLog
		{
			add
			{
				WLogHandler -= value;
				WLogHandler += value;
			}
			remove
			{
				WLogHandler -= value;
			}
		}

		public static void Log( string id, string str )
		{
			Log( id, str, LogType.DEBUG );
		}

		public static void Log( string str, LogType p, Signal s )
		{
			VSLog( str, p );
			if ( WLogHandler != null )
			{
				Task.Factory.StartNew( () => WLogHandler( new LogArgs( str, p, s ) ) );
			}
		}

		public static void Log( string id, string str, LogType p )
		{
#if DEBUG
			if( p == LogType.ERROR )
			{
				Debugger.Break();
			}
#endif
			VSLog( str, p );
			if ( WLogHandler != null )
			{
				if ( 0 < LogFilter.Count && !LogFilter.Contains( p ) ) return;

				Task.Factory.StartNew( () => WLogHandler( new LogArgs( id, str, p, Signal.LOG ) ) );
			}
		}

		private static void VSLog( string str, LogType p )
		{
			int b = ( int ) p;
			if ( 49 < b && b < 60 )
				System.Diagnostics.Debug.WriteLine( str );
		}

	}
}
