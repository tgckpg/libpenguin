using System.ComponentModel;

namespace Net.Astropenguin.DataModel
{
	public class ActiveData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyChanged( string Name )
		{
			if ( PropertyChanged != null )
				PropertyChanged( this, new PropertyChangedEventArgs( Name ) );
		}

        protected void NotifyChanged( params string[] Names )
        {
            // Must check each time after property changed is called
            // PropertyChanged may be null after event call
            foreach( string Name in Names ) NotifyChanged( Name );
        }
	}
}
