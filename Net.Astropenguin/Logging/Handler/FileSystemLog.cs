using System.IO;
using System.Text;
using System.IO.IsolatedStorage;

using Net.Astropenguin.IO;

namespace Net.Astropenguin.Logging.Handler
{
    public class FileSystemLog
    {
        protected IsolatedStorageFileStream LogFile;

        public string Location { get; private set; }

        public FileSystemLog( string path )
        {
            Location = path;
            Start();
        }

        public void Stop()
        {
            Logger.OnLog -= Logger_OnLog;
            LogFile.Dispose();
        }

        public IsolatedStorageFileStream GetStream()
        {
            IsolatedStorageFile isf = new AppStorage().GetISOStorage();
            return new IsolatedStorageFileStream( Location, FileMode.Open, isf );
        }

        public void Start()
        {
            IsolatedStorageFile isf = new AppStorage().GetISOStorage();
            LogFile = new IsolatedStorageFileStream( Location, FileMode.Append, isf );
            Logger.OnLog += Logger_OnLog;
        }

        private void Logger_OnLog( LogArgs LogArgs )
        {
            lock ( LogFile )
            {
                byte[] b = Encoding.UTF8.GetBytes( LogArgs.LogLine + "\n" );
                LogFile.Write( b, 0, b.Length );
                LogFile.Flush();
            }
        }
    }
}