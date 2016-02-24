using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Net.Astropenguin.IO
{
    public static class XRegistryExt
    {
        #region Node Functions
        public static XParameter GetFirstParameterWithKey( this XElement Root, string key )
        {
            XElement p = Root.FindFirstParameterWithKey( key );
            if ( p != null )
                return new XParameter( p );
            else return null;
        }

        public static XParameter GetParameter( this XElement Root, string WIdentifier )
        {
            XElement p = Root.FindParameter( WIdentifier );
            if ( p != null )
                return new XParameter( p );
            else return null;
        }

        public static XParameter[] GetParameters( this XElement Root )
        {
            int l;
            IEnumerable<XElement> p = Root.Elements( XRegistry.WTAG );
            if ( 0 < ( l = p.Count() ) )
            {
                XParameter[] w = new XParameter[ l ];
                for ( int i = 0; i < l; i++ )
                {
                    w[ i ] = new XParameter( p.ElementAt( i ) );
                }
                return w;
            }
            return new XParameter[ 0 ];
        }

        public static XParameter[] GetParametersWithKey( this XElement Root, string key )
        {
            IEnumerable<XElement> xe = Root.Elements( XRegistry.WTAG )
                .Where( p => p.Attribute( key ) != null );
            if ( xe == null ) return new XParameter[ 0 ];

            xe = xe.ToArray();
            int l;
            XParameter[] ps = new XParameter[ l = xe.Count() ];
            for ( int i = 0; i < l; i++ )
            {
                ps[ i ] = new XParameter( xe.ElementAt( i ) );
            }
            return ps;
        }

        public static void SetParameter( this XElement Root, string WIdentifier, XKey[] keys, XParameter[] Params = null )
        {
            XElement tag = Root.FindParameter( WIdentifier );
            if ( tag != null )
            {
                foreach ( XKey k in keys ) tag.SetKey( k );
                if ( Params != null ) foreach ( XParameter P in Params ) tag.SetParameter( P );
            }
            else
            {
                Root.Add( new XElement(
                    XRegistry.WTAG
                    , new XAttribute[] {
                        new XAttribute( XRegistry.WIDENTIFIER, WIdentifier ) }
                        .Concat( keys ), Params )
                );
            }
        }

        public static void SetParameter( this XElement Root, string WIdentifier, XKey key )
        {
            XElement tag = Root.FindParameter( WIdentifier );
            if ( tag != null )
            {
                tag.SetKey( key );
            }
            else
            {
                Root.Add( new XElement( XRegistry.WTAG, new XAttribute[] { new XAttribute( XRegistry.WIDENTIFIER, WIdentifier ), key } ) );
            }
        }

        public static void SetParameter( this XElement Root, XParameter Param )
        {
            XElement P = Root.FindParameter( Param.ID );
            if ( P == null )
            {
                Root.SetParameter( Param.ID, Param.Keys, Param.GetParameters() );
            }
            else
            {
                P.ClearKeys(); P.ClearParams();
                P.SetXValue( Param.Keys );

                foreach( XParameter Pr in Param.Params )
                    P.SetParameter( Pr );
            }
        }

        public static void SetParameter( this XElement Root, IEnumerable<XParameter> Params )
        {
            foreach( XParameter Param in Params )
            {
                Root.SetParameter( Param );
            }
        }

        public static void RemoveParameter( this XElement Root, string WIdentifier )
        {
            XElement tag = Root.FindParameter( WIdentifier );
            if ( tag != null ) tag.Remove();
        }

        public static XElement FindParameter( this XElement Root, string WIdentifier )
        {
            IEnumerable<XElement> xe = Root.Elements( XRegistry.WTAG );
            foreach ( XElement k in xe )
            {
                if ( k.Attribute( XRegistry.WIDENTIFIER ).Value == WIdentifier )
                {
                    return k;
                }
            }
            return null;
        }

        private static void SetKey( this XElement tag, XKey key )
        {
            XAttribute xa = tag.Attribute( key.KeyName );
            if ( xa != null )
            {
                xa.Value = key.KeyValue;
            }
            else
            {
                tag.Add( key );
            }
        }

        private static XElement FindFirstParameterWithKey( this XElement Root, string key )
        {
            IEnumerable<XElement> xe = Root.Elements( XRegistry.WTAG );
            foreach ( XElement k in xe )
            {
                if ( k.Attribute( key ) != null )
                {
                    return k;
                }
            }
            return null;
        }
        #endregion

        #region Attribute Functions
        public static string GetXValue( this XElement XRef, string key )
        {
            XAttribute xa = XRef.Attribute( key );
            if ( xa != null )
            {
                return xa.Value;
            }
            return null;
        }

        public static bool GetBool( this XElement XRef, string key, bool d = false )
        {
            string v = XRef.GetXValue( key );
            if ( v == null ) return d;

            int B;

            if ( int.TryParse( v, out B ) )
            {
                return B != 0;
            }
            return false;
        }

        public static long GetSaveLong( this XElement XRef, string key )
        {
            long B = 0;
            long.TryParse( XRef.GetXValue( key ), out B );
            return B;
        }

        public static int GetSaveInt( this XElement XRef, string key )
        {
            int B = 0;
            int.TryParse( XRef.GetXValue( key ), out B );
            return B;
        }

        public static void SetXValue( this XElement XRef, XKey key )
        {
            XAttribute xa = XRef.Attribute( key.KeyName );
            if ( xa != null )
            {
                xa.Value = key.KeyValue;
            }
            else
            {
                XRef.Add( key );
            }
        }

        public static void SetXValue( this XElement XRef, params XKey[] keys )
        {
            foreach ( XKey key in keys ) XRef.SetXValue( key );
        }

        public static void RemoveKey( this XElement XRef, string keyName )
        {
            XAttribute xa = XRef.Attribute( keyName );
            if ( xa != null )
                xa.Remove();
        }

        public static void ClearKeys( this XElement XRef )
        {
            foreach ( XAttribute xa in XRef.Attributes().ToArray() )
            {
                if ( xa.Name == XRegistry.WIDENTIFIER ) continue;
                xa.Remove();
            }
        }

        public static void ClearParams( this XElement XRef )
        {
            foreach ( XElement xa in XRef.Elements().ToArray() )
            {
                xa.Remove();
            }
        }
        #endregion

        public static IEnumerable<XParameter> ToXParam( this IEnumerable<string> StrSet, string KeyName = "Value", string Prefix = "" )
        {
            List<XParameter> XParams = new List<XParameter>();

            int i = 0;
            foreach ( string str in StrSet )
            {
                XParameter Param = new XParameter( Prefix + ( i++ ) );
                Param.SetValue( new XKey( KeyName, str ) );
                XParams.Add( Param );
            }

            return XParams;
        }

        public static XParameter FindFirstMatch( this XDocument Doc, string Key, string Value )
        {
            XElement Elem = Doc.Descendants().FirstOrDefault( x =>
            {
                XAttribute Attr = x.Attribute( Key );
                if ( Attr == null || Attr.Value != Value ) return false;

                return true;
            } );

            if ( Elem == null ) return null;
            return new XParameter( Elem );
        }
    }
}
