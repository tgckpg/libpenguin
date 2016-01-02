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
            :base(
                 AStorage.FileExists( Location )
                     ? FromDocument( Xml, Location )
                     : Parse( Xml )
             )
        {
            this.Location = Location;
        }

		public void Save()
		{
            if( !string.IsNullOrEmpty( Location ) )
            {
                try
                {
                    AStorage.WriteString( Location, base.ToString() );
                }
                catch( global::System.Exception ex )
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

		public void RemoveParameter( string WIdentifier )
		{
			XElement tag = FindParameter( WIdentifier );
			if ( tag != null ) tag.Remove();
		}

		public void RemoveKey( string WIdentifier, string[] keys )
		{
			XElement tag = FindParameter( WIdentifier );
			if ( tag != null )
			{
				foreach ( string k in keys )
				{
					RemoveKey( tag, k );
				}
			}
		}

		public XKey GetKey( string WIdentifier, string key )
		{
			XElement tag = FindParameter( WIdentifier );
			if ( tag != null )
			{
				XAttribute xa = tag.Attribute( key );
				if ( xa != null )
				{
					return ( XKey ) xa;
				}
			}
			return null;
		}

		public XParameter GetParameter( string WIdentifier )
		{
			XElement p = FindParameter( WIdentifier );
			if ( p != null )
				return new XParameter( p );
			else return null;
		}

		public XParameter[] GetParameters()
		{
			int l;
			IEnumerable<XElement> p = this.Root.Descendants( WTAG );
			if ( 0 < ( l = p.Count() ) )
			{
				XParameter[] w = new XParameter[l];
				for ( int i = 0; i < l; i++ )
				{
					w[i] = new XParameter( p.ElementAt( i ) );
				}
				return w;
			}
			return new XParameter[0];
		}

        // I am LHS, Always favor Master
        public void Sync( XRegistry MergeReg, bool IsMaster, global::System.Func<XParameter, XParameter, bool> LHSWin )
        {
            XParameter[] LHSs = GetParameters();
            XParameter[] RHSs = MergeReg.GetParameters();

            IEnumerable<XParameter> All = new List<XParameter>( LHSs ).Concat( RHSs );

            foreach( XParameter US in All )
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
                    if( !LHSWin( LHS, RHS ) ) SetParameter( RHS );
                }
            }
        }

        public void Merge( XRegistry MergeReg, global::System.Func<XParameter, XParameter, bool> LHSWin )
        {
            XParameter[] LHSs = GetParameters();
            XParameter[] RHSs = MergeReg.GetParameters();

            IEnumerable<XParameter> All = new List<XParameter>( LHSs ).Concat( RHSs );

            foreach( XParameter US in All )
            {
                XParameter LHS = LHSs.Contains( US ) ? US : GetParameter( US.ID );
                XParameter RHS = RHSs.Contains( US ) ? US : MergeReg.GetParameter( US.ID );

                if ( LHS == null )
                {
                    SetParameter( RHS );
                }
                else if ( !( LHS == null || RHS == null ) )
                {
                    if( !LHSWin( LHS, RHS ) ) SetParameter( RHS );
                }
            }
        }

        public void PutLast( string WIdentifier )
		{
			XElement p = FindParameter( WIdentifier );
			if ( p != null )
			{
				p.Remove();
				this.Root.Add( p );
			}
		}

		public XParameter GetFirstParameterWithKey( string key )
		{
			XElement p = FindFirstParameterWithKey( key );
			if ( p != null )
				return new XParameter( p );
			else return null;
		}

        public XParameter[] GetParametersWithKey( string key )
        {
            IEnumerable<XElement> xe = this.Root.Descendants( WTAG ).Where( p => p.Attribute( key ) != null );
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

		public int CountParametersWithKey( string key )
		{
			IEnumerable<XElement> xe = this.Root.Descendants( WTAG );
			int i = 0;
			foreach ( XElement k in xe )
			{
				if ( k.Attribute( key ) != null )
					i++;
			}
			return i;
		}

		public void RemoveKey( string WIdentifier, string key )
		{
			XElement tag = FindParameter( WIdentifier );
			if ( tag != null ) RemoveKey( tag, key );
		}

		public void SetParameter( string WIdentifier, XKey key )
		{
			XElement tag = FindParameter( WIdentifier );
			if ( tag != null )
			{
				SetKey( tag, key );
			}
			else
			{
				this.Root.Add( new XElement( WTAG, new XAttribute[] { new XAttribute( WIDENTIFIER, WIdentifier ) , key } ) );
			}
		}

		public bool ParameterExists( string WIdentifier )
		{
			IEnumerable<XElement> xe = this.Root.Descendants( WTAG );
			foreach ( XElement k in xe )
			{
				if ( k.Attribute( WIDENTIFIER ) .Value == WIdentifier )
				{
					return true;
				}
			}
			return false;
		}

		public void SetParameter( string WIdentifier, XKey[] keys )
		{
			XElement tag = FindParameter( WIdentifier );
			if ( tag != null )
			{
				foreach ( XKey k in keys )
					SetKey( tag, k );
			}
			else
			{
				this.Root.Add( new XElement( WTAG, new XAttribute[] { new XAttribute( WIDENTIFIER, WIdentifier ) }.Concat( keys ) ) );
			}
		}

        public void SetParameter( XParameter Param )
        {
            XParameter P = GetParameter( Param.ID );
            if ( P == null )
            {
                SetParameter( Param.ID, Param.Keys );
            }
            else
            {
                P.ClearKeys();
                P.SetValue( Param.Keys );
            }
        }

		private XElement FindParameter( string WIdentifier )
		{
			IEnumerable<XElement> xe = this.Root.Descendants( WTAG );
			foreach( XElement k in xe ) {
				if ( k.Attribute( WIDENTIFIER ) .Value == WIdentifier )
				{
					return k;
				}
			}
			return null;
		}

		private XElement FindFirstParameterWithKey( string key )
		{
			IEnumerable<XElement> xe = this.Root.Descendants( WTAG );
			foreach ( XElement k in xe )
			{
				if ( k.Attribute( key ) != null )
				{
					return k;
				}
			}
			return null;
		}

		private void SetKey( XElement tag, XKey key )
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

		private void RemoveKey( XElement tag, string k )
		{
			XAttribute xa = tag.Attribute( k );
			if ( xa != null )
				xa.Remove();
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
                catch( global::System.Exception )
                {
                    Logger.Log( ID, "File is corrupted: " + Path, LogType.ERROR );
                }


                try
                {
                    AStorage.DeleteFile( Path );
                }
                catch( global::System.Exception )
                {
                    Logger.Log( ID, "Unable to remove the corrupted file: " + Path, LogType.ERROR );
                }
            }

            return new XDocument( FallBack );
        }

	}

	public class XParameter
	{
		private XElement XRef;

        public string ID
        {
            get
            {
                return GetValue( XRegistry.WIDENTIFIER );
            }
        }

        public XKey[] Keys
        {
            get
            {
                int l = XRef.Attributes().Count();
                XKey[] K = new XKey[ l - 1 ];
                int i = 0;
                foreach( XAttribute attr in XRef.Attributes() )
                {
                    if ( attr.Name == XRegistry.WIDENTIFIER ) continue;
                    K[ i++ ] = new XKey( attr.Name.ToString(), attr.Value );
                }
                return K;
            }
        }

		public XParameter( XElement e ) { XRef = e; }

		public string GetValue( string key )
		{
			XAttribute xa = XRef.Attribute( key );
			if ( xa != null )
			{
				return xa.Value;
			}
			return null;
		}

        public bool GetBool( string key, bool d = false )
        {
            string v = GetValue( key );
            if ( v == null ) return d;

            int B;

            if ( int.TryParse( v, out B ) )
            {
                return B != 0;
            }
            return false;
        }

        public long GetSaveLong( string key )
        {
            long B = 0;
            long.TryParse( GetValue( key ), out B );
            return B;
        }

        public int GetSaveInt( string key )
        {
            int B = 0;
            int.TryParse( GetValue( key ), out B );
            return B;
        }

		public void SetValue( XKey key )
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

        public void SetValue( params XKey[] keys )
        {
            foreach( XKey key in keys ) SetValue( key );
        }

		public void RemoveKey( string keyName )
		{
			XAttribute xa = XRef.Attribute( keyName );
			if ( xa != null )
				xa.Remove();
		}

        public void ClearKeys()
        {
            foreach ( XAttribute xa in XRef.Attributes().ToArray() )
            {
                if ( xa.Name == XRegistry.WIDENTIFIER ) continue;
                xa.Remove();
            }
        }
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

		public XKey( string key, string value ) : base( key, value ) { }

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
