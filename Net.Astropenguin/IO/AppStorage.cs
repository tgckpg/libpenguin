using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

using Net.Astropenguin.Logging;
using Net.Astropenguin.Helpers;

namespace Net.Astropenguin.IO
{
	public class AppStorage
	{
		public static readonly string ID = typeof( AppStorage ).Name;

		private static Dictionary<int, IStorageFile> StaticCaches = new Dictionary<int, IStorageFile>();

		public static StorageItemAccessList FutureAccessList
		{
			get { return StorageApplicationPermissions.FutureAccessList; }
		}

		public static async Task<StorageFile> MkTemp( string FileName = "tmp" )
		{
			return await ApplicationData.Current.TemporaryFolder.CreateFileAsync( FileName, CreationCollisionOption.GenerateUniqueName );
		}

		public static async Task<IStorageFile> StaticTemp( IStorageFile SrcFile )
		{
			int FileHash = SrcFile.Path.GetHashCode();
			if ( StaticCaches.ContainsKey( FileHash ) )
			{
				return StaticCaches[ FileHash ];
			}

			IStorageFile SF = await SrcFile.CopyAsync( ApplicationData.Current.TemporaryFolder, "static", NameCollisionOption.GenerateUniqueName );
			StaticCaches[ FileHash ] = SF;

			return SF;
		}

		public static async Task<byte[]> AppXGetBytes( string path )
		{
			IStorageFile ISF = await StorageFile.GetFileFromApplicationUriAsync( new Uri( "ms-appx:///" + path ) );
			IBuffer Buff = await FileIO.ReadBufferAsync( ISF );

			return Buff.ToArray();
		}

		public static async Task<IStorageFolder> OpenDirAsync( Action<FolderPicker> PickerHandler = null )
		{
			try
			{
				FolderPicker fpick = new FolderPicker();
				fpick.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
				fpick.FileTypeFilter.Add( "*" );
				PickerHandler?.Invoke( fpick );

				StorageFolder folder = await fpick.PickSingleFolderAsync();

				return folder;
			}
			catch ( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.ERROR );
			}

			return null;
		}

		public static Task<IStorageFile> OpenFileAsync( string FileExt ) => OpenFileAsync( new string[] { FileExt } );

		public static async Task<IStorageFile> OpenFileAsync( IEnumerable<string> FileExts )
		{
			try
			{
				FileOpenPicker fpick = new FileOpenPicker();
				fpick.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
				foreach ( string ext in FileExts ) fpick.FileTypeFilter.Add( ext );

				StorageFile file = await fpick.PickSingleFileAsync();

				return file;
			}
			catch( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.ERROR );
			}

			return null;
		}

		public static async Task<IStorageFile> SaveFileAsync( string Name, IList<string> Types, string FileName = "" )
		{
			try
			{
				FileSavePicker fpick = new FileSavePicker();
				fpick.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
				fpick.FileTypeChoices.Add( Name, Types );
				fpick.SuggestedFileName = FileName;

				StorageFile file = await fpick.PickSaveFileAsync();

				return file;
			}
			catch( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.ERROR );
			}

			return null;
		}

		public static async Task ClearTemp()
		{
			try
			{
				IEnumerable<IStorageFile> ISFs = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();

				foreach ( IStorageFile ISF in ISFs )
				{
					await ISF.DeleteAsync();
				}
			}
			catch( Exception ex )
			{
				Logger.Log( ID, ex.Message, LogType.ERROR );
			}
		}

		protected IsolatedStorageFile UserStorage;
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
			if ( !UserStorage.DirectoryExists( Dir ) )
				return;

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
				IStorageFolder folder = await OpenDirAsync();
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

		public void CountSizeRecursive( string folder, ref ulong size )
		{
			foreach ( string DirName in UserStorage.GetDirectoryNames( folder ) )
			{
				CountSizeRecursive( folder + DirName + "/", ref size );
			}
			CountSize( folder, ref size );
		}

		public async Task<ulong> CountSizeRecursive( IStorageFolder folder )
		{
			BasicProperties prop = await folder.GetBasicPropertiesAsync();
			ulong size = prop.Size;

			IEnumerable<IStorageFolder> dirs = await folder.GetFoldersAsync();
			foreach ( IStorageFolder dir in dirs ) size += await CountSizeRecursive( dir );

			IEnumerable<IStorageFile> files = await folder.GetFilesAsync();
			foreach ( IStorageFile file in files )
			{
				prop = await file.GetBasicPropertiesAsync();
				size += prop.Size;
			}

			return size;
		}

		public void CountSize( string folder, ref ulong size )
		{
			foreach ( string fileName in UserStorage.GetFileNames( folder ) )
			{
				IsolatedStorageFileStream file = UserStorage.OpenFile( folder + fileName, FileMode.Open, FileAccess.Read );
				size += ( ulong ) file.Length;
				file.Dispose();
			}
		}

		public void CreateDirectory( string Name ) => UserStorage.CreateDirectory( Name );
		public bool DirExist( string Name ) => UserStorage.DirectoryExists( Name );

		public string[] ListDirs( string List )
		{
			return UserStorage.GetDirectoryNames( List );
		}

		public string[] ListFiles( string Dir )
		{
			return UserStorage.GetFileNames( Dir );
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

		public async Task<IStorageFolder> CreateDirFromISOStorage( string Location )
		{
			string[] Folders = Location.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries );

			IStorageFolder DirStack = await ApplicationData.Current.LocalFolder.CreateFolderAsync( Folders[ 0 ], CreationCollisionOption.OpenIfExists );

			int l = Folders.Length;
			for ( int i = 1; i< l; i++ )
			{
				DirStack = await DirStack.CreateFolderAsync( Folders[ i ], CreationCollisionOption.OpenIfExists );
			}

			return DirStack;
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

		public byte[] GetBytes( string fileName )
		{
			if ( FileExists( fileName ) )
			{
				using ( MemoryStream ms = new MemoryStream() )
				using ( Stream s = GetStream( fileName ) )
				{
					s.CopyTo( ms );
					return ms.ToArray();
				}
			}

			return null;
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

		virtual public bool WriteString( string fileName, string Content )
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

		public bool CreateDirs( string dir )
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

		virtual public bool WriteBytes( string fileName, Byte[] bytes )
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

		virtual public bool WriteStream( string fileName, Stream s )
		{
			try
			{
				using ( Stream StreamData = s )
				{
					byte[] buffer = new byte[ 1024 ];
					using ( IsolatedStorageFileStream isfs = new IsolatedStorageFileStream( fileName, FileMode.Create, UserStorage ) )
					{
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