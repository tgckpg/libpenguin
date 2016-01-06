using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.IO
{
    // Registry Class
    // Does many things, save settings to XMLs
    public class XRegistry : XDocument
    {
        public static readonly string ID = typeof( XRegistry ).Name;
        public static AppStorage AStorage;

        public const string WIDENTIFIER = "WS_ID";
        public const string WTAG = "WTAG";

        public string Location { get; private set; }

        public XRegistry( string Xml, string Location )
            : base(
                 AStorage.FileExists( Location )
                     ? FromDocument( Xml, Location )
                     : Parse( Xml )
             )
        {
            this.Location = Location;
        }

        public void Save()
        {
            if ( !string.IsNullOrEmpty( Location ) )
            {
                try
                {
                    AStorage.WriteString( Location, base.ToString() );
                }
                catch ( global::System.Exception ex )
                {
                    Logger.Log(
                        ID
                        , string.Format(
                            "Failed to save {0}, things might not work properly: {1}"
                            , Location, ex.Message
                        )
                        , LogType.ERROR
                    );
                }
            }
            else
            {
                Logger.Log( ID, "Location is not specified, cannot save", LogType.WARNING );
            }
        }

        // I am LHS, Always favor Master
        public void Sync( XRegistry MergeReg, bool IsMaster, global::System.Func<XParameter, XParameter, bool> LHSWin )
        {
            XParameter[] LHSs = GetParameters();
            XParameter[] RHSs = MergeReg.GetParameters();

            IEnumerable<XParameter> All = new List<XParameter>( LHSs ).Concat( RHSs );

            foreach ( XParameter US in All )
            {
                XParameter LHS = LHSs.Contains( US ) ? US : GetParameter( US.ID );
                XParameter RHS = RHSs.Contains( US ) ? US : MergeReg.GetParameter( US.ID );

                if ( LHS == null && !IsMaster )
                {
                    SetParameter( RHS );
                }
                else if ( RHS == null && !IsMaster )
                {
                    RemoveParameter( LHS.ID );
                }
                else if ( !( LHS == null || RHS == null ) )
                {
                    if ( !LHSWin( LHS, RHS ) ) SetParameter( RHS );
                }
            }
        }

        public void Merge( XRegistry MergeReg, global::System.Func<XParameter, XParameter, bool> LHSWin )
        {
            XParameter[] LHSs = GetParameters();
            XParameter[] RHSs = MergeReg.GetParameters();

            IEnumerable<XParameter> All = new List<XParameter>( LHSs ).Concat( RHSs );

            foreach ( XParameter US in All )
            {
                XParameter LHS = LHSs.Contains( US ) ? US : GetParameter( US.ID );
                XParameter RHS = RHSs.Contains( US ) ? US : MergeReg.GetParameter( US.ID );

                if ( LHS == null )
                {
                    SetParameter( RHS );
                }
                else if ( !( LHS == null || RHS == null ) )
                {
                    if ( !LHSWin( LHS, RHS ) ) SetParameter( RHS );
                }
            }
        }

        public XParameter GetParameter( string WIdentifier ) { return Root.GetParameter( WIdentifier ); }
        public XParameter[] GetParameters() { return Root.GetParameters(); }
        public XParameter[] GetParametersWithKey( string key ) { return Root.GetParametersWithKey( key ); }
        public XParameter GetFirstParameterWithKey( string key ) { return Root.GetFirstParameterWithKey( key ); }
        public void SetParameter( string WIdentifier, XKey key ) { Root.SetParameter( WIdentifier, key ); }
        public void SetParameter( string WIdentifier, XKey[] keys ) { Root.SetParameter( WIdentifier, keys ); }
        public void SetParameter( XParameter Param ) { Root.SetParameter( Param ); }
        public void RemoveParameter( string WIdentifier ) { Root.RemoveParameter( WIdentifier ); }

        public void PutLast( string WIdentifier )
        {
            XElement p = Root.FindParameter( WIdentifier );
            if ( p != null )
            {
                p.Remove();
                this.Root.Add( p );
            }
        }

        public int CountParametersWithKey( string key )
        {
            IEnumerable<XElement> xe = this.Root.Elements( WTAG );
            int i = 0;
            foreach ( XElement k in xe )
            {
                if ( k.Attribute( key ) != null )
                    i++;
            }
            return i;
        }

        private static XDocument FromDocument( string FallBack, string Path )
        {
            string xml = AStorage.GetString( Path );

            if ( xml != null )
            {
                try
                {
                    XDocument doc = Parse( xml );
                    return doc;
                }
                catch ( global::System.Exception )
                {
                    Logger.Log( ID, "File is corrupted: " + Path, LogType.ERROR );
                }


                try
                {
                    AStorage.DeleteFile( Path );
                }
                catch ( global::System.Exception )
                {
                    Logger.Log( ID, "Unable to remove the corrupted file: " + Path, LogType.ERROR );
                }
            }

            return new XDocument( FallBack );
        }

    }

    public class XParameter : XElement
    {
        public string ID
        {
            get
            {
                return this.GetValue( XRegistry.WIDENTIFIER );
            }
            set
            {
                this.SetValue( new XKey( XRegistry.WIDENTIFIER, value ) );
            }
        }

        public XKey[] Keys
        {
            get
            {
                int l = this.Attributes().Count();
                XKey[] K = new XKey[ l - 1 ];
                int i = 0;
                foreach ( XAttribute attr in this.Attributes() )
                {
                    if ( attr.Name == XRegistry.WIDENTIFIER ) continue;
                    K[ i++ ] = new XKey( attr.Name.ToString(), attr.Value );
                }
                return K;
            }
        }

        public XParameter[] Params
        {
            get
            {
                int l = this.Elements().Count();
                XParameter[] K = new XParameter[ l ];
                int i = 0;
                foreach ( XElement elem in this.Elements() )
                {
                    K[ i++ ] = new XParameter( elem );
                }
                return K;
            }
        }

        public XParameter( XElement e ) : base( e ) { }
        public XParameter( string ID )
            : base( XRegistry.WTAG )
        {
            this.ID = ID;
        }

        public string GetValue( string key ) { return this.GetXValue( key ); }
        public void SetValue( XKey key ) { this.SetXValue( key ); }
        public void SetValue( params XKey[] keys ) { this.SetXValue( keys ); }
    }

    public static class XParamExt
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
    }

    public class XKey : XAttribute
    {
        public string KeyName
        {
            get
            {
                return Name.ToString();
            }
        }

        public string KeyValue
        {
            get { return Value; }
            set { Value = value; }
        }

        public XKey( string key, string value )
            : base( key, value == null ? "" : value )
        {
        }

        public XKey( string name, bool value )
            : this( name, value ? "1" : "0" )
        {
        }

        public XKey( string name, object value )
            : this( name, value.ToString() )
        {
        }
    }
}
