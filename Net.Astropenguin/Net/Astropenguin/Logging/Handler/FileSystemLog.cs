using System.IO;
using System.Text;
using System.IO.IsolatedStorage;

namespace Net.Astropenguin.Logging.Handler
{
	public class FileSystemLog
	{
		protected IsolatedStorageFileStream LogFile;

		public FileSystemLog( string path )
		{
			IsolatedStorageFile isf = new AppStorage().GetISOStorage();
			LogFile = new IsolatedStorageFileStream( path, FileMode.Append, isf );
			Logger.OnLog += Logger_OnLog;
		}

		private void Logger_OnLog( LogArgs LogArgs )
		{
			byte[] b = Encoding.UTF8.GetBytes( LogArgs.LogLine + "\n" );
			LogFile.Write( b, 0, b.Length );
			LogFile.Flush();
		}
	}
}
