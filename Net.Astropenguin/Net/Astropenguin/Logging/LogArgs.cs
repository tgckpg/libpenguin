using System;

namespace Net.Astropenguin.Logging
{
	public enum LogType
	{
		// Basic log levels
		SYSTEM, R1, R2, R3, R4, R5, R6, R7, R8, R9
		// Informative
		, INFO, DEBUG, R12, R13, R14, R15, R16, R17, R18, R19
		// Warnings
		, WARNING, R21, R22, R23, R24, R25, R26, R27, R28, R29
		// Errors
		, ERROR, R31, R32, R33, R34, R35, R36, R37, R38, R39
		// Other things
		, WRUNTIMETRANSFER, R41, R42, R43, R44, R45, R46, R47, R48, R49
		// Tests
		, TEST_START, TEST_END, TEST_RESULT, R53, R54, R55, R56, R57, R58, R59
	};

	public enum Signal { LOG, EXIT };

	public class LogArgs
	{
        public string id { get; private set; }
		public string Message { get; private set; }
		public LogType Type { get; private set; }
		public Signal sig { get; private set; }

		public DateTime timestamp { get; private set; }

		public string LogLine
		{
			get
			{
				string d = string.Format( "{0:MM-dd-yyyy HH:mm:ss}", timestamp );
                if ( id != null )
				return String.Format( "[{0}][{1}][{2}] {3}", d, Type, id, Message );

				return String.Format( "[{0}][{1}] {2}", d, Type, Message );
			}
		}

		public string LogStamp
		{
			get
			{
				return ( ( int ) Type ) + " " + id + " " + Message;
			}
		}

		public LogArgs( string id, string str, LogType p, Signal s )
            :this( str, p, s )
		{
            this.id = id;
		}

		public LogArgs( string str, LogType p, Signal s )
		{
			Message = str;
			Type = p;
			sig = s;
			timestamp = DateTime.Now;		   
		}

	}
}
