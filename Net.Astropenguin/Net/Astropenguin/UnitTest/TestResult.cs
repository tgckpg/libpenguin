using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.System.Threading;

namespace Net.Astropenguin.UnitTest
{
	public class TestResult
	{
		private bool? Result = null;
		private List<string> Lines;

		public string Name { get; private set; }
		public Action<TestCompleteArgs> OnComplete;

		public string Message
		{
			get
			{
				string n = "[" + Name + "][" + ( Result == true ? "1" : "0" ) + "] ";
				if( Result == true )
				{
					return n + "OK";
				}
				return n + string.Join( "\n" + n, Lines );
			}
		}

		public bool Complete
		{
			get { return Result != null; }
		}

		public TestResult( string Name )
		{
			this.Name = Name;
			Lines = new List<string>();
			ThreadPoolTimer.CreateTimer( TimeOut, new TimeSpan( 0, 0, 30 ) );
		}

		private void TimeOut( ThreadPoolTimer timer )
		{
			if( Result == null )
			{
				writeLine( "Test timed out" );
				Done( false );
			}
		}

		public void writeLine( string line )
		{
			if ( Result != null ) return;
			Lines.Add( line );
		}

		public void Done( bool status )
		{
			if ( Result != null ) return;
			Result = status;

			if( OnComplete != null )
				OnComplete( new TestCompleteArgs( this ) );
		}
	}
}
