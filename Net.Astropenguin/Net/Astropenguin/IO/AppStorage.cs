using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

using Net.Astropenguin.Logging;
using Net.Astropenguin.Helpers;
using Windows.Storage.Pickers;

namespace Net.Astropenguin.IO
{
    public class AppStorage
    {
        public static readonly string ID = typeof( AppStorage ).Name;

        public static async Task<StorageFile> MkTemp()
        {
            return await ApplicationData.Current.TemporaryFolder.CreateFileAsync( "tmp", CreationCollisionOption.GenerateUniqueName );
        }


        private IsolatedStorageFile UserStorage;
        public StorageFolder PicLibrary;

        public AppStorage()
        {
            UserStorage = IsolatedStorageFile.GetUserStoreForApplication();
        }

        public IsolatedStorageFile GetISOStorage()
        {
            return UserStorage;
        }

        public void PurgeContents( string Dir, bool DeleteRoot )
        {
            foreach ( string file in UserStorage.GetFileNames( Dir ) )
            {
                UserStorage.DeleteFile( Dir + file );
            }
            foreach ( string d in UserStorage.GetDirectoryNames( Dir ) )
            {
                PurgeContents( Dir + d + "/", true );
            }
            if ( DeleteRoot )
                UserStorage.DeleteDirectory( Dir );
        }

        public bool DeleteFile( string Location )
        {
            if ( UserStorage.FileExists( Location ) )
            {
                UserStorage.DeleteFile( Location );
                return true;
            }
            return false;
        }

        public async Task<AsyncTryOut<Stream>> TryGetImgFromLib( string id )
        {
            string PotentialError = "";
            try
            {
                IReadOnlyList<StorageFile> list = await PicLibrary.GetFilesAsync();
                foreach ( StorageFile p in list )
                {
                    if ( !p.ContentType.Contains( "image" ) ) continue;
                    PotentialError = p.Name;
                    if ( p.Name == id )
                    {
                        Stream s = await p.OpenStreamForReadAsync();
                        return new AsyncTryOut<Stream>( true, s );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logger.Log( ID, "AppStorage.TryGetImgFromLib: Could not get image from lib", LogType.ERROR );
                Logger.Log( ID, ex.Message, LogType.INFO );
                Logger.Log( ID, "Potential Error: " + PotentialError, LogType.INFO );
            }

            return new AsyncTryOut<Stream>();
        }

        public async Task<IReadOnlyList<StorageFile>> PickFolderForFiles()
        {
            try
            {
                FolderPicker fpick = new FolderPicker();
                fpick.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                fpick.FileTypeFilter.Add( ".txt" );

                StorageFolder folder = await fpick.PickSingleFolderAsync();
                if ( folder == null ) return null;

                return await folder.GetFilesAsync();
            }
            catch( Exception ex )
            {
                Logger.Log( ID, ex.Message, LogType.ERROR );
            }

            return null;
        }

        public async Task<bool> SearchLibrary( string fileName )
        {
            try
            {
                // Search the library
                IReadOnlyList<StorageFile> list = await PicLibrary.GetFilesAsync();
                foreach ( StorageFile p in list )
                {
                    if ( !p.ContentType.Contains( "image" ) ) continue;
                    if ( p.Name == fileName ) return true;
                }
            }
            catch ( Exception )
            {
                // Handles Unknown internal error from
                // System.Windows.MessageBox.Show( ex.Message );
            }
            return false;
        }

        public async Task<bool> TestLibraryValid()
        {
            try
            {
                // Test the library valid for search
                string fileName = "net/astropenguin/token";
                IReadOnlyList<StorageFile> list = await PicLibrary.GetFilesAsync();
                foreach ( StorageFile p in list ) if ( p.Name == fileName ) return true;
            }
            catch ( Exception ex )
            {
                Logger.Log( ID, "Library Test Failed: " + ex.Message, LogType.WARNING );
                return false;
            }
            return true;
        }

        public void CountSizeRecursive( string folder, ref long size )
        {
            foreach ( string DirName in UserStorage.GetDirectoryNames( folder ) )
            {
                CountSizeRecursive( folder + DirName + "/", ref size );
            }
            CountSize( folder, ref size );
        }

        public void CountSize( string folder, ref long size )
        {
            foreach ( string fileName in UserStorage.GetFileNames( folder ) )
            {
                IsolatedStorageFileStream file = UserStorage.OpenFile( folder + fileName, FileMode.Open, FileAccess.Read );
                size += file.Length;
                file.Dispose();
            }
        }

        public void CreateDirectory( string Name )
        {
            UserStorage.CreateDirectory( Name );
        }

        public bool DirExist( string Name )
        {
            return UserStorage.DirectoryExists( Name );
        }

        public string[] ListDirs( string List )
        {
            return UserStorage.GetDirectoryNames( List );
        }

        public Stream GetStream( string fileName )
        {
            // Open the file
            IsolatedStorageFileStream isfsr = UserStorage.OpenFile( fileName, FileMode.Open, FileAccess.Read );
            return isfsr;
        }

        public async Task<IStorageFile> CreateImageFromLibrary( string saveLocation )
        {
            return await PicLibrary.CreateFileAsync( saveLocation, CreationCollisionOption.ReplaceExisting );
        }

        public async Task<IStorageFile> CreateFileFromISOStorage( string saveLocation )
        {
            string[] Folders = saveLocation.Split( '/' );

            int l = Folders.Length - 1;
            IStorageFolder DirStack = await ApplicationData.Current.LocalFolder.GetFolderAsync( Folders[ 0 ] );
            for ( int i = 1; i < l; i++ )
            {
                DirStack = await DirStack.GetFolderAsync( Folders[ i ] );
            }

            return await DirStack.CreateFileAsync( Folders[ l ] );
        }

        public string GetString( string fileName )
        {
            string p = null;
            if ( FileExists( fileName ) )
            {
                using ( StreamReader ms = new StreamReader( GetStream( fileName ) ) )
                {
                    p = ms.ReadToEnd();
                }
            }
            return p;
        }

        public DateTimeOffset FileTime( string filename )
        {
            return UserStorage.GetLastWriteTime( filename );
        }

        public bool FileExists( string fileName )
        {
            try
            {
                return UserStorage.FileExists( fileName );
            }
            catch ( Exception ) { }
            return false;
        }

        public long FileSize( string fileName )
        {
            IsolatedStorageFileStream file = UserStorage.OpenFile( fileName, FileMode.Open );
            long size = file.Length;
            file.Dispose();
            return size;
        }

        public bool WriteString( string fileName, string Content )
        {
            try
            {
                // Set buffer
                using ( Stream StreamData = new MemoryStream( Encoding.UTF8.GetBytes( Content ) ) )
                {
                    byte[] buffer = new byte[ 1024 ];
                    // Write, and over write
                    using ( IsolatedStorageFileStream isfs = new IsolatedStorageFileStream( fileName, FileMode.Create, FileAccess.Write, UserStorage ) )
                    {
                        int count = 0;
                        while ( 0 < ( count = StreamData.Read( buffer, 0, buffer.Length ) ) )
                        {
                            isfs.Write( buffer, 0, count );
                        }
                    }
                }
                return true;
            }
            catch ( Exception ex )
            {
                Logger.Log( ID, "WriteString@" + fileName + ": " + ex.Message, LogType.ERROR );
                Logger.Log( ID, ex.StackTrace, LogType.INFO );
            }
            return false;
        }

        public bool createDirs( string dir )
        {
            if ( DirExist( dir ) ) return true;

            string[] s = dir.Split( '/' );
            string append = "";
            foreach ( string i in s )
            {
                if ( !( i == "" || DirExist( append += "/" + i ) ) )
                {
                    CreateDirectory( append );
                }
            }
            return DirExist( dir );
        }

        public bool WriteBytes( string fileName, Byte[] bytes )
        {
            try
            {
                using ( IsolatedStorageFileStream isfs = new IsolatedStorageFileStream( fileName, FileMode.Create, UserStorage ) )
                {
                    isfs.Write( bytes, 0, bytes.Count() );
                }
            }
            catch ( IsolatedStorageException ex )
            {
                Logger.Log(
                    ID
                    , string.Format( "WriteByte@{0}: {1}", fileName, ex.Message )
                    , LogType.ERROR
                );
                return false;
            }
            return true;
        }

        public bool WriteStream( string fileName, Stream s )
        {
            try
            {
                // Get Stream
                using ( Stream StreamData = s )
                {
                    // Set buffer
                    byte[] buffer = new byte[ 1024 ];
                    // Save the file into cache
                    using ( IsolatedStorageFileStream isfs = new IsolatedStorageFileStream( fileName, FileMode.Create, UserStorage ) )
                    {
                        // This write the stream into file
                        int count = 0;
                        while ( 0 < ( count = StreamData.Read( buffer, 0, buffer.Length ) ) )
                        {
                            isfs.Write( buffer, 0, count );
                        }
                    }
                }
            }
            catch ( IsolatedStorageException )
            {
                return false;
            }
            return true;
        }

        public async Task InitAppLibrary( string Name )
        {
            try
            {
                StorageFolder S = KnownFolders.SavedPictures;

                IStorageItem Item = await S.TryGetItemAsync( Name );
                if ( Item == null )
                {
                    PicLibrary = await S.CreateFolderAsync( Name );
                }
                else if ( Item.IsOfType( StorageItemTypes.Folder ) )
                {
                    PicLibrary = ( StorageFolder ) Item;
                }
                else
                {
                    throw new Exception( string.Format( "PicLibrary: {0} is not a folder" ) );
                }
            }
            catch ( Exception ex )
            {
                Logger.Log( ID, "AppLibrary Initialization failed: " + ex.Message, LogType.WARNING );
            }
        }
    }
}
