using System.Globalization;
using Windows.ApplicationModel.Resources;

namespace Net.Astropenguin.Loaders
{
	public class StringResources
	{
		protected ResourceLoader ResCont;
        public string Language { get; internal set; }
        public CultureInfo Culture = CultureInfo.CurrentUICulture;

		public StringResources()
		{
			ResCont = ResourceLoader.GetForCurrentView( "AppResources" );
		}

		public StringResources( string View )
		{
			ResCont = ResourceLoader.GetForCurrentView( View );
		}

		public string Text( string Key )
		{
			return ResCont.GetString( Key + "/Text" );
		}

		public string Str( string Key )
		{
			return ResCont.GetString( Key );
		}
	}
}
