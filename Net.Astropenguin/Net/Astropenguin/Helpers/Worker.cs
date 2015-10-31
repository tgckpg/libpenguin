using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Net.Astropenguin.Logging;
using Windows.UI.Core;

namespace Net.Astropenguin.Helpers
{
	public class Worker
	{
        public static readonly string ID = typeof( Worker ).Name;
		static BackgroundWorker bw;

		static Action[] ActionList;
		const int l = 256;
		static int i = 0;

		public static void Initialize()
		{
			ActionList = new Action[l];
			bw = new BackgroundWorker();
			bw.WorkerSupportsCancellation = true;
			bw.DoWork += async ( sender, e ) =>
			{
				Logger.Log( ID, "Work Cycle Started", LogType.INFO );
				// Global background working cycle
				while ( -1 < ( i = GetNextWorkIndex() ) )
				{
					ActionList[ i ]();
					ActionList[ i ] = null;
					// Each cycle rest for 200ms
					await Task.Delay( TimeSpan.FromMilliseconds( 200 ) );

				}
				Logger.Log( ID, "Work Cycle Complete", LogType.INFO );
			};
		}

		public static int GetNextWorkIndex()
		{
			for ( i = 0; i < l; i++ )
			{
				if ( ActionList[i] != null )
					return i;
			}
			return -1;
		}

		public static void ReisterBackgroundWork( Action Work )
		{
			Logger.Log( ID, "Registering Work", LogType.INFO );
			RegisterAction( Work );
			if ( !bw.IsBusy )
			{
				Logger.Log( ID, "Worker idle, fire working signal.", LogType.INFO );
				bw.RunWorkerAsync();
			}
		}

		private static bool RegisterAction( Action Work )
		{
			for ( int j = 0; j < l; j++ )
			{
				if ( ActionList[j] == null )
				{
					ActionList[j] = Work;
					return true;
				}
			}
			return false;
		}

		public static void TerminateBackgroundWork()
		{
			if ( bw.WorkerSupportsCancellation )
			{
				Logger.Log( ID, "Work Cycle Canceled", LogType.INFO );
				ActionList = new Action[l];
				bw.CancelAsync();
			}
		}

		public static async void UIInvoke( Action p )
		{
			CoreWindow cw = null;
			try
			{
				cw = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow;
			}
			catch( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.ERROR );
			}

			if ( cw != null )
				await cw.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, new DispatchedHandler( p ) );
			else throw new Exception( "Unable to get UI Thread" );
		}
	}
}
