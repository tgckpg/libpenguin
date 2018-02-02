using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Net.Astropenguin.Logging;
using Windows.UI.Core;
using System.Collections.Concurrent;

namespace Net.Astropenguin.Helpers
{
	public class Worker
	{
		public static readonly string ID = typeof( Worker ).Name;
		static BackgroundWorker bw;

		private static CoreWindow CoreUIInstance = null;
		private static Stack<Action> SuspendedList = new Stack<Action>();

		private static ConcurrentQueue<Action> ActionQueue;

		public static bool BackgroundOnly { get; internal set; }

		public static void Initialize()
		{
			ActionQueue = new ConcurrentQueue<Action>();
			bw = new BackgroundWorker();
			bw.WorkerSupportsCancellation = true;
			bw.DoWork += Bw_DoWork;
			bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
		}

		private static void Bw_DoWork( object sender, DoWorkEventArgs e )
		{
			while ( ActionQueue.TryDequeue( out Action Work ) )
				Work();
		}

		private static void Bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			Logger.Log( ID, string.Format( "Worker Completed ({0}): {1}, {2}", e.Cancelled ? "Canceled" : "Done", e.Error, e.Result ), LogType.DEBUG );
		}

		public static void ReisterBackgroundWork( Action Work )
		{
			ActionQueue.Enqueue( Work );
			if ( !bw.IsBusy )
			{
				Logger.Log( ID, "Starting Worker ...", LogType.INFO );
				bw.RunWorkerAsync();
			}
		}

		public static void TerminateBackgroundWork()
		{
			if ( bw.WorkerSupportsCancellation )
			{
				Logger.Log( ID, "Work Cycle Canceled", LogType.INFO );
				bw.CancelAsync();
				ActionQueue = new ConcurrentQueue<Action>();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Await.Warning", "CS4014:Await.Warning" )]
		public static void UIInvoke( Action p )
		{
			RunUIAsync( p );
		}

		public static async Task RunUIAsync( Action p )
		{
			if ( CoreUIInstance == null )
			{
				SSLog( "MainView's CoreWindow is Null, trying to get one" );

				try
				{
					CoreUIInstance = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow;
				}
				catch ( Exception ex )
				{
					SSLog( ex.Message );
				}

				if ( CoreUIInstance == null )
				{
					SSLog( "Cannot get a CoreWindow. Suspending this action" );
					SuspendedList.Push( p );
					Logger.Log( ID, string.Format( "Now we have {0} suspended actions", SuspendedList.Count ), LogType.INFO );
					return;
				}

				Logger.Log( ID, "Hurray, got the CoreWindow. Let's resume in operations.", LogType.INFO );
			}

			if ( 0 < SuspendedList.Count )
			{
				Logger.Log( ID, "Dispatching Suspended actions", LogType.INFO );

				foreach ( Action s in SuspendedList )
				{
					await CoreUIInstance.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, new DispatchedHandler( s ) );
				}

				SuspendedList.Clear();
			}

			try
			{
				await CoreUIInstance.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, new DispatchedHandler( p ) );
			}
			catch ( Exception e )
			{
				Logger.Log( ID, "Action Dispatched an Error", LogType.SYSTEM );
				Logger.Log( ID, e.Message, LogType.ERROR );
			}
		}

		private static void SSLog( string Mesg )
		{
			if ( SuspendedList.Any() ) return;
			Logger.Log( ID, Mesg, LogType.INFO );
		}
	}
}