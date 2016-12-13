using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace Net.Astropenguin.DataModel
{
    using Loaders;
    using Logging;
    using Helpers;

    public class Observables<IN, OUT> : ObservableCollection<OUT>, ISupportIncrementalLoading
    {
        public static readonly string ID = typeof( Observables<IN, OUT> ).Name;

        public event EventHandler LoadStart;
        public event EventHandler LoadEnd;

        private volatile bool UniLoader = true;
        private ILoader<IN> ActiveLoader;
        private Func<IList<IN>, IList<OUT>> Convert;

        private Stack<ILoader<IN>> SubLoaders = new Stack<ILoader<IN>>();
        private Dictionary<ILoader<IN>, OUT> LastAnchor = new Dictionary<ILoader<IN>, OUT>();

        private IAsyncOperation<LoadMoreItemsResult> CurrentAsyncOp;

        virtual public bool HasMoreItems
        {
            get
            {
                if ( ActiveLoader == null ) return false;

                // Get the suspended loader when ActiveLoader is exhausted
                if ( ActiveLoader.PageEnded )
                {
                    RestoreLoader();
                }

                return !ActiveLoader.PageEnded;
            }
        }

        public Observables() : base() { }

        public Observables( IList<OUT> Items )
            : base( Items )
        {
        }

        public void UpdateSource( IList<OUT> Items )
        {
            this.ClearItems();
            LockAdd( Items, ActiveLoader );
        }

        virtual public void ConnectLoader( ILoader<IN> Loader, Func<IList<IN>, IList<OUT>> Converter = null )
        {
            ActiveLoader = Loader;
            Loader.Connector = null;
            Convert = Converter;
        }

        virtual public void DisconnectLoaders()
        {
            if( ActiveLoader != null )
            {
                ActiveLoader.Connector = null;
                ActiveLoader = null;
                Convert = null;
            }

            foreach ( ILoader<IN> Loader in SubLoaders )
                Loader.Connector = null;
        }

        virtual public void InsertLoader( int Idx, ILoader<IN> SubLoader )
        {
            UniLoader = false;
            SubLoaders.Push( ActiveLoader );
            LastAnchor[ SubLoader ] = this.ElementAtOrDefault( Idx );
            ActiveLoader = SubLoader;
        }

        virtual protected uint LockAdd( IList<OUT> Items, ILoader<IN> Loader )
        {
            if ( Items.Count == 0 ) return 0;

            lock ( this )
            {
                if ( UniLoader || this.Count == 0 || LastAnchor[ Loader ] == null )
                {
                    foreach ( OUT Item in Items )
                        this.Add( Item );
                }
                else
                {
                    int i = this.IndexOf( LastAnchor[ Loader ] );

                    foreach ( OUT Item in Items )
                    {
                        this.Insert( i++, Item );
                    }

                    LastAnchor[ Loader ] = Items.Last();
                }
            }

            return ( uint ) Items.Count;
        }

        private void RestoreLoader()
        {
            if ( SubLoaders == null || SubLoaders.Count == 0 ) return;

            ILoader<IN> CurrentLoader = ActiveLoader;

            while ( 0 < SubLoaders.Count && ActiveLoader.PageEnded )
            {
                CurrentLoader = SubLoaders.Pop();
                LastAnchor.Remove( CurrentLoader );
            }

            ActiveLoader = CurrentLoader;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync( uint count )
        {
            if ( CurrentAsyncOp == null || CurrentAsyncOp.Status != AsyncStatus.Started )
            {
                CurrentAsyncOp = LoadNext( ActiveLoader, count ).AsAsyncOperation();
            }

            return CurrentAsyncOp;
        }

        private async Task<LoadMoreItemsResult> LoadNext( ILoader<IN> CurrLoader, uint count )
        {
            Logger.Log( ID, string.Format( "Requesting to load {0} items, Current Page is {1}", count, CurrLoader.CurrentPage ) );

            if ( LoadStart != null )
                Worker.UIInvoke( () => LoadStart( this, new EventArgs() ) );

            TaskCompletionSource<IList<IN>> NextItems = new TaskCompletionSource<IList<IN>>();

            // Might get set twice via the connector and the returned result
            // But it is safe
            CurrLoader.Connector = x =>
            {
                if ( x == null ) return;
                NextItems.TrySetResult( x );
            };

            CurrLoader.Connector( await CurrLoader.NextPage( count ) );

            IList<IN> Items = await NextItems.Task;

            IList<OUT> Converted = Convert == null
                ? ( IList<OUT> ) Items
                : Convert( Items )
            ;

            uint NumAdded = 0;

            await Worker.RunUIAsync( () =>
            {
                NumAdded = LockAdd( Converted, CurrLoader );
            } );

            Logger.Log( ID, string.Format( "Loaded {0} item(s)", NumAdded ) );

            if ( LoadEnd != null )
                Worker.UIInvoke( () => LoadEnd( this, new EventArgs() ) );

            return new LoadMoreItemsResult() { Count = NumAdded };
        }
    }
}