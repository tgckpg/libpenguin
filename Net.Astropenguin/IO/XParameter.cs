using System;
using System.Linq;
using System.Xml.Linq;

namespace Net.Astropenguin.IO
{
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
}
