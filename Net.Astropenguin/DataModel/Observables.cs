using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ILoader<IN> ConnectedLoader;
        private Func<IList<IN>, IList<OUT>> Convert;

        public bool HasMoreItems
        {
            get
            {
                if( ConnectedLoader == null ) return false;
                return !ConnectedLoader.PageEnded;
            }
        }

        public Observables() : base() { }

        public Observables( IList<OUT> Items )
            :base( Items )
        {
        }

        public void UpdateSource( IList<OUT> Items )
        {
            this.ClearItems();
            foreach( OUT O in Items )
            {
                Add( O );
            }
        }

        public void ConnectLoader( ILoader<IN> Loader, Func<IList<IN>, IList<OUT>> Converter = null )
        {
            ConnectedLoader = Loader;
            Loader.Connector = null;
            Convert = Converter;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync( uint count )
        {
            Logger.Log( ID, string.Format( "Requesting to load {0} items, Current Page is {1}", count, ConnectedLoader.CurrentPage ) );

            return Task.Run( async () =>
            {
                if ( LoadStart != null )
                    Worker.UIInvoke( () => LoadStart( this, new EventArgs() ) );

                TaskCompletionSource<IList<IN>> NextItems = new TaskCompletionSource<IList<IN>>();

                // Maybe set twice via connector and returned result
                // But this is safe
                ConnectedLoader.Connector = x =>
                {
                    if ( x == null ) return;
                    NextItems.TrySetResult( x );
                };

                ConnectedLoader.Connector( await ConnectedLoader.NextPage( count ) );

                IList<IN> Items = await NextItems.Task;

                IList<OUT> Converted = Convert == null
                    ? ( IList<OUT> ) Items
                    : Convert( Items )
                ;

                TaskCompletionSource<uint> ItemsAdded = new TaskCompletionSource<uint>();

                // It seemed that items must be added via UI Thread
                Worker.UIInvoke( () =>
                {
                    uint i = 0;
                    foreach ( OUT a in Converted )
                    {
                        Add( a ); i++;
                    }
                    ItemsAdded.SetResult( i );
                } );

                uint Count = await ItemsAdded.Task;

                Logger.Log( ID, string.Format( "Loaded {0} item(s)", Count ) );

                if ( LoadEnd != null )
                    Worker.UIInvoke( () => LoadEnd( this, new EventArgs() ) );

                return new LoadMoreItemsResult() { Count = Count };
            } ).AsAsyncOperation();
        }
    }
}
