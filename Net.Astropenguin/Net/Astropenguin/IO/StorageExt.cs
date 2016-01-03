using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.IO
{
    public static class StorageExt
    {
        public static readonly string ID = typeof( StorageExt ).Name;

        public async static Task<string> ReadString( this IStorageFile ISF )
        {
            IInputStream ips = await ISF.OpenSequentialReadAsync();
            StreamReader Reader = new StreamReader( ips.AsStreamForRead() );
            return Reader.ReadToEnd();
        }

        public async static Task<bool> WriteString( this IStorageFile ISF, string Content )
        {
            try
            {
                // Write, and over write
                using ( Stream StreamData = await ISF.OpenStreamForWriteAsync() )
                {
                    byte[] b = Encoding.UTF8.GetBytes( Content );
                    await StreamData.WriteAsync( b, 0, b.Length );
                }
                return true;
            }
            catch ( Exception ex )
            {
                Logger.Log( ID, "WriteString@" + ISF.Name + ": " + ex.Message, LogType.ERROR );
                Logger.Log( ID, ex.StackTrace, LogType.INFO );
            }

            return false;
        }
    }
}
