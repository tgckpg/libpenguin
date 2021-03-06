﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Net.Astropenguin.Helpers
{
	public class Popups
	{
		private static IAsyncOperation<ContentDialogResult> DialogCommand = null;
		private static IAsyncOperation<IUICommand> MsgDialogCommand = null;

		public async static Task<bool> ShowDialog( ContentDialog dlg )
		{
			// Close the previous one out
			CloseDialog();

			dlg.Closed += Dlg_Closed;
			DialogCommand = dlg.ShowAsync();
			try
			{
				await DialogCommand;
			}
			catch ( OperationCanceledException )
			{

			}

			return true;
		}

		private static void Dlg_Closed( ContentDialog sender, ContentDialogClosedEventArgs args )
		{
			sender.Closed -= Dlg_Closed;
			if( sender is IDisposable )
			{
				( sender as IDisposable ).Dispose();
			}
		}

		public async static Task<bool> ShowDialog( MessageDialog dlg )
		{
			// Close the previous one out
			if ( MsgDialogCommand != null )
			{
				MsgDialogCommand.Cancel();
				MsgDialogCommand = null;
			}

			MsgDialogCommand = dlg.ShowAsync();
			try
			{
				await MsgDialogCommand;
			}
			catch( OperationCanceledException )
			{

			}

			return true;
		}

		public static bool CloseDialog()
		{
			if ( DialogCommand != null )
			{
				DialogCommand.Cancel();
				DialogCommand = null;
				return true;
			}

			return false;
		}
	}
}
