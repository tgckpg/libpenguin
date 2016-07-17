using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel.Resources;

namespace Net.Astropenguin.Loaders
{
	public class StringResources
	{
		protected Dictionary<string,ResourceLoader> ResCont;

        public string Language { get; internal set; }
        public CultureInfo Culture = CultureInfo.CurrentUICulture;

        protected ResourceLoader DefaultRes;

        public StringResources( params string[] Views )
        {
            if ( Views.Length == 0 )
            {
                DefaultRes = ResourceLoader.GetForCurrentView( "AppResources" );
                return;
            }

            ResCont = new Dictionary<string, ResourceLoader>();
            foreach ( string View in Views )
                ResCont.Add( View, ResourceLoader.GetForCurrentView( View ) );

            DefaultRes = ResCont[ Views[ 0 ] ];
        }

        public string Text( string Key ) { return DefaultRes.GetString( Key + "/Text" ); }
        public string Text( string Key, string View ) { return ResCont[ View ].GetString( Key + "/Text" ); }

        public string Str( string Key ) { return DefaultRes.GetString( Key ); }
        public string Str( string Key, string View ) { return ResCont[ View ].GetString( Key ); }
	}
}
