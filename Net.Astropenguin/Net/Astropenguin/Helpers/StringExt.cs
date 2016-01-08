using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Net.Astropenguin.Helpers
{
    public static class StringExt
    {
        public static Dictionary<string, string> EscMap = new Dictionary<string, string>() {
            { "n", "\n" }, { "r", "\r" }, { "t", "\t" }, { "f", "\f" }
            , { "a", "\a" }, { "b", "\b" }
        };

        /// <summary>
        /// Unescape backslashed representations.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string Unescape( this string v )
        {
            Regex R = new Regex( "\\\\(.)" );
            return R.Replace( v, ( x ) =>
            {
                string X = x.Groups[ 1 ].Value;
                if ( EscMap.ContainsKey( X ) ) return EscMap[ X ];
                return X;
            } );
        }
    }
}
