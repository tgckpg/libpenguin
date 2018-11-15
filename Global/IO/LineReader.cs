using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.IO
{
	public class LineReader : StreamReader
	{
		public long LinePos { get; private set; } = 0;

		private bool _r;

		public LineReader( Stream stream ) : base( stream ) { }

		public void SeekLine( long i )
		{
			while ( !EndOfStream )
			{
				if ( LinePos == i ) break;
				base.ReadLine();
				LinePos++;
			}
		}

		public override string ReadLine()
		{
			try
			{
				_r = true;
				return base.ReadLine();
			}
			catch( Exception ex )
			{
				_r = false;
				throw ex;
			}
			finally
			{
				if ( _r )
				{
					LinePos++;
				}
			}
		}

	}
}