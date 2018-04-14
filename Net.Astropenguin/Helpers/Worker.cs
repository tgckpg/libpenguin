using System;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.Helpers
{
	public class Worker
	{
		public static readonly string ID = typeof( Worker ).Name;

		private BackgroundWorker BgWorker;
		private ConcurrentQueue<Action> TaskQueue;
		private string Name;

		public Worker( string Name )
		{
			this.Name = Name;

			TaskQueue = new ConcurrentQueue<Action>();
			BgWorker = new BackgroundWorker();
			BgWorker.WorkerSupportsCancellation = true;
			BgWorker.DoWork += DoWork;
			BgWorker.RunWorkerCompleted += WorkCompleted;
		}

		private void DoWork( object sender, DoWorkEventArgs e )
		{
			int i = 0;
			while ( TaskQueue.TryDequeue( out Action Work ) )
			{
				Work();
				i++;
			}
			e.Result = i;
		}

		private void WorkCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			Logger.Log( ID, string.Format( "Worker Completed ({0}): {1}, {2}. {3}", Name, e.Cancelled ? "Cancelled" : "Done", e.Result, e.Error ), LogType.DEBUG );
			if ( TaskQueue.Any() )
			{
				if ( !BgWorker.IsBusy )
					BgWorker.RunWorkerAsync();
			}
		}

		public void Queue( Action Work )
		{
			TaskQueue.Enqueue( Work );
			if ( !BgWorker.IsBusy )
			{
				BgWorker.RunWorkerAsync();
				Logger.Log( ID, "Worker Started: " + Name, LogType.DEBUG );
			}
		}

		public void Cancel()
		{
			if ( BgWorker.WorkerSupportsCancellation )
			{
				BgWorker.CancelAsync();
			}
		}

		private static volatile Worker Instance;

		public static void Register( Action Work )
		{
			if ( Instance == null )
			{
				Instance = new Worker( "Global" );
			}

			Instance.Queue( Work );
		}

		struct StackedTask
		{
			public TaskCompletionSource<bool> TCS;
			public Func<Task> Work;
			public Action WorkSync;
		}

		private static CoreWindow CoreUIInstance = null;
		private static ConcurrentStack<StackedTask> TaskList = new ConcurrentStack<StackedTask>();
		public static bool BackgroundOnly { get; internal set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Await.Warning", "CS4014:Await.Warning" )]
		public static void UIInvoke( Action p ) => RunUIAsync( p );

		public static async Task RunUIAsync( Action p )
		{
			if ( !CanCoreUI() )
			{
				StackedTask s = new StackedTask() { TCS = new TaskCompletionSource<bool>(), WorkSync = p };
				TaskList.Push( s );
				await s.TCS.Task;
				return;
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

		public static async Task RunUITaskAsync( Func<Task> p )
		{
			StackedTask K = new StackedTask() { TCS = new TaskCompletionSource<bool>(), Work = p };

			if( !CanCoreUI() )
			{
				TaskList.Push( K );
				await K.TCS.Task;
				return;
			}

			RunTask( K );
			await K.TCS.Task;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Await.Warning", "CS4014:Await.Warning" )]
		private static void RunTask( StackedTask s )
		{
			CoreUIInstance.Dispatcher.RunAsync( CoreDispatcherPriority.Normal, new DispatchedHandler( async () =>
			{
				s.WorkSync?.Invoke();

				if ( s.Work != null )
				{
					await s.Work();
				}

				s.TCS.SetResult( true );
			} ) );
		}

		private static bool CanCoreUI()
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
					Logger.Log( ID, string.Format( "Now we have {0} suspended actions", TaskList.Count ), LogType.INFO );
					return false;
				}

				Logger.Log( ID, "Hurray, got the CoreWindow. Let's resume in operations.", LogType.INFO );
			}

			if ( TaskList.Any() )
			{
				Logger.Log( ID, "Dispatching suspended tasks", LogType.INFO );

				while ( TaskList.TryPop( out StackedTask s ) )
					RunTask( s );
			}

			return true;
		}

		private static void SSLog( string Mesg )
		{
			if ( TaskList.Any() ) return;
			Logger.Log( ID, Mesg, LogType.INFO );
		}
	}
}