﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

		public async static Task<string[]> ReadLines( this IStorageFile ISF, int Count = 1 )
		{
			IInputStream ips = await ISF.OpenSequentialReadAsync();
			StreamReader Reader = new StreamReader( ips.AsStreamForRead() );
			string[] Lines = new string[ Count ];

			for ( int i = 0; i < Count && !Reader.EndOfStream; i++ )
			{
				Lines[ i ] = Reader.ReadLine();
			}

			return Lines;
		}

		public async static Task<byte[]> ReadAllBytes( this IStorageFile ISF )
		{
			IBuffer Buff = await FileIO.ReadBufferAsync( ISF );
			return Buff.ToArray();
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

		public async static Task<bool> WriteFile( this IStorageFile ISF, IStorageFile Source, bool Append = false, byte[] AdBytes = null )
		{
			try
			{
				if ( Append )
				{
					// Write and Append
					using ( Stream StreamData = await ISF.OpenStreamForWriteAsync() )
					using ( Stream SourceData = await Source.OpenStreamForReadAsync() )
					{
						StreamData.Seek( 0, SeekOrigin.End );

						await SourceData.CopyToAsync( StreamData );
						if ( AdBytes != null ) await StreamData.WriteAsync( AdBytes, 0, AdBytes.Length );
						await StreamData.FlushAsync();
					}
				}
				else
				{
					// Write truncate
					using ( Stream StreamData = await ISF.OpenStreamForWriteAsync() )
					using ( Stream SourceData = await Source.OpenStreamForReadAsync() )
					{
						await SourceData.CopyToAsync( StreamData );
						if ( AdBytes != null ) await StreamData.WriteAsync( AdBytes, 0, AdBytes.Length );
						await StreamData.FlushAsync();
					}
				}

				return true;
			}
			catch ( Exception ex )
			{
				Logger.Log( ID, "WriteFile@" + ISF.Name + ": " + ex.Message, LogType.ERROR );
				Logger.Log( ID, ex.StackTrace, LogType.INFO );
			}

			return false;
		}

		public async static Task<bool> WriteBytes( this IStorageFile ISF, byte[] Bytes, bool Append = false, byte[] AdBytes = null )
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
						if ( AdBytes != null ) await StreamData.WriteAsync( AdBytes, 0, AdBytes.Length );
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
						if ( AdBytes != null ) await StreamData.WriteAsync( AdBytes, 0, AdBytes.Length );
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