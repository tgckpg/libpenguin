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
		public LineReader( Stream stream )
			:base( stream )
		{
		}

		private long _linePos = 0;
		public long LinePos { get { return _linePos; } }

		public void SeekLine( long i )
		{
			while( !EndOfStream )
			{
				if( LinePos == i ) break;
				ReadLine();
			}
		}

		public override string ReadLine()
		{
			string Line = base.ReadLine();
			_linePos++;

			return Line;
		}

	}

}