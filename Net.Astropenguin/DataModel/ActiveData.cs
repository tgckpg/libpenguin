﻿using System.ComponentModel;

namespace Net.Astropenguin.DataModel
{
    using Helpers;

	public class ActiveData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyChanged( params string[] Names )
        {
            Worker.UIInvoke( () =>
            {
                // Must check each time after property changed is called
                // PropertyChanged may be null after event call
                foreach ( string Name in Names )
                {
                    if ( PropertyChanged != null )
                        PropertyChanged( this, new PropertyChangedEventArgs( Name ) );
                }
            } );
        }
	}
}