using Net.Astropenguin.Linq;
using System.Collections.Generic;
using System.Globalization;
using Windows.ApplicationModel.Resources;

namespace Net.Astropenguin.Loaders
{
	public class StringResources
	{
		protected static Dictionary<string, ResourceLoader> BgResCont = new Dictionary<string, ResourceLoader>();

		protected StringResources() { }

		public static StringResources Load( params string[] Views )
		{
			StringResources ResBg = new StringResources();
			if ( Views.Length == 0 )
			{
				_Load( "AppResources" );
				ResBg.DefaultRes = BgResCont[ "AppResources" ];
			}
			else
			{
				Views.ExecEach( x => _Load( x ) );
				ResBg.DefaultRes = BgResCont[ Views[ 0 ] ];
			}

			return ResBg;
		}

		protected static void _Load( string View )
		{
			if ( !BgResCont.ContainsKey( View ) )
			{
				BgResCont[ View ] = ResourceLoader.GetForViewIndependentUse( View );
			}
		}

		public string Language { get; internal set; }
		public CultureInfo Culture = CultureInfo.CurrentUICulture;

		protected ResourceLoader DefaultRes;

		public string Text( string Key ) { return DefaultRes.GetString( Key + "/Text" ); }
		public string Text( string Key, string View ) { return BgResCont[ View ].GetString( Key + "/Text" ); }

		public string Header( string Key ) { return DefaultRes.GetString( Key + "/Header" ); }
		public string Header( string Key, string View ) { return BgResCont[ View ].GetString( Key + "/Header" ); }

		public string Str( string Key ) { return DefaultRes.GetString( Key ); }
		public string Str( string Key, string View ) { return BgResCont[ View ].GetString( Key ); }

	}
}