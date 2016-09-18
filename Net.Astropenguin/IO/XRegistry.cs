using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.IO
{
    // Registry Class
    // Does many things, save settings to XMLs
    public partial class XRegistry : XDocument
    {
        public static readonly string ID = typeof( XRegistry ).Name;
        public static AppStorage AStorage;

        public const string XID = "WS_ID";
        public const string WTAG = "WTAG";

        public string Location { get; set; }

        public XRegistry( string Xml, string Location, bool ReadFromLocation = true )
            : base(
                 ( ReadFromLocation && AStorage.FileExists( Location ) )
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
                catch ( Exception ex )
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
        public void Sync( XRegistry MergeReg, bool IsMaster, Func<XParameter, XParameter, bool> LHSWin )
        {
            XParameter[] LHSs = Parameters();
            XParameter[] RHSs = MergeReg.Parameters();

            IEnumerable<XParameter> All = new List<XParameter>( LHSs ).Concat( RHSs );

            foreach ( XParameter US in All )
            {
                XParameter LHS = LHSs.Contains( US ) ? US : Parameter( US.Id );
                XParameter RHS = RHSs.Contains( US ) ? US : MergeReg.Parameter( US.Id );

                if ( LHS == null && !IsMaster )
                {
                    SetParameter( RHS );
                }
                else if ( RHS == null && !IsMaster )
                {
                    RemoveParameter( LHS.Id );
                }
                else if ( !( LHS == null || RHS == null ) )
                {
                    if ( !LHSWin( LHS, RHS ) ) SetParameter( RHS );
                }
            }
        }

        public void Merge( XRegistry MergeReg, Func<XParameter, XParameter, bool> LHSWin )
        {
            XParameter[] LHSs = Parameters();
            XParameter[] RHSs = MergeReg.Parameters();

            IEnumerable<XParameter> All = new List<XParameter>( LHSs ).Concat( RHSs );

            foreach ( XParameter US in All )
            {
                XParameter LHS = LHSs.Contains( US ) ? US : Parameter( US.Id );
                XParameter RHS = RHSs.Contains( US ) ? US : MergeReg.Parameter( US.Id );

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

        public XParameter Parameter( string WIdentifier ) { return Root.Parameter( WIdentifier ); }
        public XParameter[] Parameters() { return Root.GetParameters(); }
        public XParameter[] Parameters( string key, string value = null ) { return Root.Parameters( key, value ); }
        public XParameter FirstParameter( string key ) { return Root.FirstParameter( key ); }
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
                Root.Add( p );
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
                catch ( Exception )
                {
                    Logger.Log( ID, "File is corrupted: " + Path, LogType.ERROR );
                }


                try
                {
                    AStorage.DeleteFile( Path );
                }
                catch ( Exception )
                {
                    Logger.Log( ID, "Unable to remove the corrupted file: " + Path, LogType.ERROR );
                }
            }

            return new XDocument( FallBack );
        }

    }
}