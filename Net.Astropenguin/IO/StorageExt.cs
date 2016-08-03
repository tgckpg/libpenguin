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

        public static async Task<IStorageFolder> GetFolderAsync( this IStorageFolder Folder, string Location, bool Save )
        {
            try
            {
                string[] Folders = Location.Split( '/' );

                int l = Folders.Length;
                IStorageFolder DirStack = await ApplicationData.Current.LocalFolder.GetFolderAsync( Folders[ 0 ] );
                for ( int i = 1; i < l; i++ )
                {
                    DirStack = await DirStack.GetFolderAsync( Folders[ i ] );
                }

                return DirStack;
            }
            catch ( Exception ex )
            {
                if ( !Save ) throw ex;
            }

            return null;
        }

        public async static Task<string> ReadString( this IStorageFile ISF )
        {
            IInputStream ips = await ISF.OpenSequentialReadAsync();
            StreamReader Reader = new StreamReader( ips.AsStreamForRead() );
            return Reader.ReadToEnd();
        }

        public async static Task<string> ReadString( this IStorageFile ISF, Encoding Encoding )
        {
            IInputStream ips = await ISF.OpenSequentialReadAsync();
            StreamReader Reader = new StreamReader( ips.AsStreamForRead(), Encoding );
            return Reader.ReadToEnd();
        }

        public async static Task<bool> WriteString( this IStorageFile ISF, string Content, bool Append = false )
        {
            return await ISF.WriteBytes( Encoding.UTF8.GetBytes( Content ), Append );
        }

        public async static Task<bool> WriteBytes( this IStorageFile ISF, byte[] Bytes, bool Append = false )
        {
            try
            {
                if ( Append )
                {
                    // Write and Append
                    using ( Stream StreamData = await ISF.OpenStreamForWriteAsync() )
                    {
                        StreamData.Seek( 0, SeekOrigin.End );

                        await StreamData.WriteAsync( Bytes, 0, Bytes.Length );
                        await StreamData.FlushAsync();
                    }
                }
                else
                {
                    // Write truncate
                    using ( Stream StreamData = await ISF.OpenStreamForWriteAsync() )
                    {
                        StreamData.SetLength( Bytes.Length );

                        await StreamData.WriteAsync( Bytes, 0, Bytes.Length );
                        await StreamData.FlushAsync();
                    }
                }
                return true;
            }
            catch ( Exception ex )
            {
                Logger.Log( ID, "WriteBytes@" + ISF.Name + ": " + ex.Message, LogType.ERROR );
                Logger.Log( ID, ex.StackTrace, LogType.INFO );
            }

            return false;
        }
    }
}