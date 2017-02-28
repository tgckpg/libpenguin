using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Net.Astropenguin.Logging;
using Windows.UI.Core;

namespace Net.Astropenguin.Helpers
{
    public class Worker
    {
        public static readonly string ID = typeof( Worker ).Name;
        static BackgroundWorker bw;

        private static CoreWindow CoreUIInstance = null;
        private static Stack<Action> SuspendedList = new Stack<Action>();

        static Action[] ActionList;
        const int l = 256;
        static int i = 0;

        public static bool BackgroundOnly { get; internal set; }

        public static void Initialize()
        {
            ActionList = new Action[ l ];
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
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
        }

        private static void Bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            Logger.Log( ID, string.Format( "Completed ({0}): {1}, {2}", e.Cancelled ? "Canceled" : "Done", e.Error, e.Result ), LogType.DEBUG );
        }

        public static int GetNextWorkIndex()
        {
            for ( i = 0; i < l; i++ )
            {
                if ( ActionList[ i ] != null )
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
                if ( ActionList[ j ] == null )
                {
                    ActionList[ j ] = Work;
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
                ActionList = new Action[ l ];
                bw.CancelAsync();
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