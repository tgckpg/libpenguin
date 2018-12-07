using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Net.Astropenguin.IO
{
	public static class StreamExt
	{
		public static DeflateStream AsInflateStream( this Stream s )
		{
			return new DeflateStream( s, CompressionMode.Decompress );
		}

		public static DeflateStream AsDeflateStream( this Stream s )
		{
			return new DeflateStream( s, CompressionLevel.Optimal, true );
		}
	}
}