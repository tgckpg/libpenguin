using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.UnitTest
{
	public class TestExecutor
	{
		public static readonly string ID = typeof( TestExecutor ).Name;
		protected List<TestResult> Results;

		private int TestsToWait = 0;
		private int TestsWaited = 0;
		protected void Run()
		{
			MethodInfo[] mi = Type.GetType( GetType().FullName ).GetMethods( BindingFlags.Public | BindingFlags.Instance );
			Regex r = new Regex( "^Test_(.+)" );

			Results = new List<TestResult>();

			foreach ( MethodInfo m in mi )
			{
				Match mt = r.Match( m.Name );
				if ( mt.Success )
				{
					TestResult t = new TestResult( mt.Groups[ 1 ].Value );
					Logger.Log( ID, "Start " + mt.Groups[ 1 ].Value, LogType.TEST_START );

					m.Invoke( this, new TestResult[] { t } );
					if ( !t.Complete )
					{
						t.OnComplete += CheckDone;
						TestsToWait++;
					}
					Results.Add( t );
				}
			}

			CheckDone();
		}

		protected void CheckDone( TestCompleteArgs a )
		{
			TestsWaited++;
			CheckDone();
		}

		protected void CheckDone()
		{
			if( TestsToWait == TestsWaited )
			{
				Logger.Log( ID, "Test Ended", LogType.TEST_END );
				foreach( TestResult t in Results )
				{
					Logger.Log( ID, t.Message, LogType.TEST_RESULT );
				}
			}
		}

	}
}