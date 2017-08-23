using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Net.Astropenguin.Loaders
{
	using IO;
	using Logging;

	class StorageFileStreamer : ILoader<string>
	{
		public static readonly string ID = typeof( StorageFileStreamer ).Name;

		private StorageFile File;

		public Action<IList<string>> Connector { get; set; }
		public int CurrentPage { get; set; }
		public bool PageEnded { get; set; }

		private long CurrentPos = 0;

		public async Task<IList<string>> NextPage( uint count )
		{
			return await OpenRead( ( ulong ) CurrentPos, count );
		}

		public StorageFileStreamer( StorageFile SF )
		{
			File = SF;
		}

		private async Task<IList<string>> OpenRead( ulong start = 0, uint lines = 10 )
		{
			List<string> p = new List<string>();

			try
			{
				IInputStream s = await File.OpenSequentialReadAsync();

				using ( LineReader ms = new LineReader( s.AsStreamForRead() ) )
				{
					ms.SeekLine( CurrentPos );
					for( int i = 0; i < lines; i ++ )
					{
						if ( ms.EndOfStream ) break;
						string line = ms.ReadLine();

						// This is needed for ListView igoring empty lines
						if ( string.IsNullOrEmpty( line ) ) line = " ";

						p.Add( line );
					}

					PageEnded = ms.EndOfStream;
					CurrentPos = ms.LinePos;
				}
			}
			catch( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.ERROR );
			}

			return p;
		}
	}
}
