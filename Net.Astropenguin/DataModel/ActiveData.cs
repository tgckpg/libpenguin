using System.ComponentModel;

namespace Net.Astropenguin.DataModel
{
	using Helpers;

	public class ActiveData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyChanged( params string[] Names ) => _NotifyChanged( this, Names );
		protected void _NotifyChanged( object sender, params string[] Names )
		{
			if ( Worker.BackgroundOnly ) return;

			Worker.UIInvoke( () =>
			{
				// Must check each time after property changed is called
				// PropertyChanged may be null after event call
				foreach ( string Name in Names )
				{
					PropertyChanged?.Invoke( sender, new PropertyChangedEventArgs( Name ) );
				}
			} );
		}
	}
}