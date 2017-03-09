using System.Collections.Generic;
using System.Linq;

namespace Net.Astropenguin.UI
{
	class VerticalLogaManager
	{
		private static Dictionary<double, VerticalLogaTable> LogaCache = new Dictionary<double, VerticalLogaTable>();

		public static VerticalLogaTable GetLoga( double FontSize )
		{
			VerticalLogaTable L;
			if ( !LogaCache.ContainsKey( FontSize ) )
			{
				L = new VerticalLogaTable( FontSize );
				LogaCache.Add( FontSize, L );
			}
			else
			{
				L = LogaCache[ FontSize ];
			}

			return L;
		}

		public static void Destroy( double FontSize )
		{
			LogaCache.Remove( FontSize );
		}

	}
}