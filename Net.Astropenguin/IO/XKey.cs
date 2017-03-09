using System.Xml.Linq;

namespace Net.Astropenguin.IO
{
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