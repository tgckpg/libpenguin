using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Net.Astropenguin.Logging;

namespace Net.Astropenguin.UI
{
	[TemplatePart( Name = StageName, Type = typeof( StackPanel ) )]
	public class VerticalStack : Control
	{
		public static readonly string ID = typeof( VerticalStack ).Name;
		private static readonly Size SIZE_NULL = new Size( 0, 0 );
		private const int LOGA_BECAME_CERTAIN = 10;

		public const string StageName = "Stage";

		internal static int UpdateDelay = 500;
		private volatile bool DLock = false;

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( VerticalStack ), new PropertyMetadata( "", VisualPropertyChanged ) );
		public string Text
		{
			get { return GetValue( TextProperty ) as string; }
			set { SetValue( TextProperty, value ); }
		}

		new public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register( "FontSize", typeof( double ), typeof( VerticalStack ), new PropertyMetadata( 0.0, VisualPropertyChanged ) );
		new public double FontSize
		{
			get { return ( double ) GetValue( FontSizeProperty ); }
			set { SetValue( FontSizeProperty, value ); }
		}

		public static readonly DependencyProperty TrimProperty = DependencyProperty.Register( "Trim", typeof( bool ), typeof( VerticalStack ), new PropertyMetadata( false, VisualPropertyChanged ) );
		public bool Trim
		{
			get { return ( bool ) GetValue( TrimProperty ); }
			set { SetValue( TrimProperty, value ); }
		}

		public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register( "MaxLines", typeof( int ), typeof( VerticalStack ), new PropertyMetadata( 0, VisualPropertyChanged ) );
		public int MaxLines
		{
			get { return ( int ) GetValue( MaxLinesProperty ); }
			set { SetValue( MaxLinesProperty, value ); }
		}

		public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register( "LineHeight", typeof( double ), typeof( VerticalStack ), new PropertyMetadata( -1.0, LineHeightChanged ) );
		public double LineHeight
		{
			get { return ( double ) GetValue( LineHeightProperty ); }
			set { SetValue( LineHeightProperty, value ); }
		}

		protected Size GivenSizeAvailable { get; private set; }

		private static void VisualPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			( d as VerticalStack ).UpdateDisplay();
		}

		private static void LineHeightChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			( d as VerticalStack ).RedrawLineHeight();
		}

		private StackPanel Stage;

		public VerticalStack()
			: base()
		{
			DefaultStyleKey = typeof( VerticalStack );

			// Perhaps a bad idea for cached pages?
			// Unloaded += DisposeStage;
		}

		private void DisposeStage( object sender, RoutedEventArgs e )
		{
			// ClearStage();
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			Stage = GetTemplateChild( StageName ) as StackPanel;
			UpdateDisplay();
		}

		protected override Size MeasureOverride( Size availableSize )
		{
			/* INTENSIVE_LOG
			Logger.Log(
				ID
				, string.Format( "MeasureOverride {{ W: {0}, H: {1} }}", availableSize.Width, availableSize.Height )
				, LogType.DEBUG
			);
			//*/
			// If the Size Changes, we need to update the text
			// TODO: Use a more efficient approach
			if ( !GivenSizeAvailable.Equals( availableSize ) )
			{
				// Get the available size from parent
				GivenSizeAvailable = new Size( availableSize.Width, availableSize.Height );
				DelaySizeUpdate();
			}

			return base.MeasureOverride( availableSize );
		}

		private async void DelaySizeUpdate()
		{
			if ( UpdateDelay == 0 )
			{
				UpdateDisplay();
				return;
			}

			if ( DLock ) return;
			DLock = true;
			await Task.Delay( UpdateDelay );
			UpdateDisplay();
			DLock = false;
		}

		private void UpdateDisplay()
		{
			/* INTENSIVE_LOG
			Logger.Log( ID, "UpdateDisplay", LogType.DEBUG );
			//*/

			ClearStage();
			// I can't draw nothing, so... remove everything?
			if ( GivenSizeAvailable.Height == 0
				|| string.IsNullOrEmpty( Text ) || FontSize == 0 || LineHeight < 0 ) return;

			DrawTextBlocks( Text );
		}

		private void ClearStage()
		{
			if ( Stage == null ) return;
			Stage.Children.Clear();

			/* INTENSIVE_LOG
			Logger.Log( ID, "Stage Cleared " + Stage.Children.Count, LogType.DEBUG );
			//*/
		}

		private void RedrawLineHeight()
		{
			if ( Stage == null ) return;

			if ( Stage.Children.Count == 0 && !string.IsNullOrEmpty( Text ) )
			{
				UpdateDisplay();
				return;
			}

			foreach ( TextBlock Tx in Stage.Children )
			{
				double lh = -.5 * FontSize + LineHeight;
				Tx.Margin = new Thickness( lh, 0, lh, 0 );
			}
		}

		private void DrawTextBlocks( string Text )
		{
			TextBlock t = NewTextBlock( Text );

			TextBlock Tx;

			if ( Trim && MaxLines != 0 )
			{
				int i = 0; int l = MaxLines;
				TextBlock Last = null;
				while ( ( Tx = TrimText( t ) ) != null && i < l )
				{
					// Two special char to ensure the center alignment
					Tx.Text += "\u3000\u2007";
					Stage.Children.Add( Tx );

					Last = Tx;
					i++;
				}

				if( 0 < t.Text.Length && Last != null )
				{
					Last.Text = Last.Text.Substring( 0, Last.Text.Length - 3 ) + "\u22EE\u3000\u2007";
					t.Text = "";
				}
			}
			else
			{
				while ( ( Tx = TrimText( t ) ) != null )
				{
					// Two special char to ensure the center alignment
					Tx.Text += "\u3000\u2007";
					Stage.Children.Add( Tx );
				}
			}

			/* INTENSIVE_LOG
			if( Tx != null )
			{
				Logger.Log(
					ID
					, string.Format( "LastText is: {0}", Tx.Text )
					, LogType.DEBUG
				);
			}
			//*/

			// Does the MasterTextBlock still have some text?
			if ( 0 < t.Text.Length )
			{
				/* INTENSIVE_LOG
				Logger.Log(
					ID
					, string.Format( "TextBlock still have texts: {0}", t.Text )
					, LogType.DEBUG
				);
				//*/
				t.Text += "\u3000\u2007";
				Stage.Children.Add( t );
			}
		}

		private TextBlock TrimText( TextBlock t )
		{
			if ( double.IsInfinity( GivenSizeAvailable.Height ) )
			{
				return null;
			}

			TextBlock TrimmedText = null;

			int EstTrimLen = 0;

			VerticalLogaTable Table = VerticalLogaManager.GetLoga( FontSize );
			if ( LOGA_BECAME_CERTAIN < Table.CertaintyLevel )
			{
				EstTrimLen = Table.GetTrimLenForHeight( GivenSizeAvailable.Height );

				if ( EstTrimLen == 0 || t.Text.Length < EstTrimLen || t.Text == "" ) return null;

				TrimmedText = NewTextBlock(
					t.Text.Substring( 0, EstTrimLen )
				);
			}
			else
			{
				BlockHeightOf( t );
				EstTrimLen = ( int ) Math.Floor( t.Text.Length * GivenSizeAvailable.Height / t.ActualHeight );

				if ( EstTrimLen == 0 || t.Text.Length < EstTrimLen || t.Text == "" ) return null;

				TrimmedText = NewTextBlock(
					t.Text.Substring( 0, EstTrimLen )
				);

				// Add text one by one
				int i = 0;
				while ( BlockHeightOf( TrimmedText ) < GivenSizeAvailable.Height
					&& ( EstTrimLen + i ) < t.Text.Length
				)
				{
					TrimmedText.Text += t.Text[ EstTrimLen + ( i++ ) ];
				}

				// Then remove them one by one to get the best estimation
				while ( GivenSizeAvailable.Height < BlockHeightOf( TrimmedText ) )
				{
					TrimmedText.Text = TrimmedText.Text.Substring( 0, EstTrimLen + ( --i ) );
				}

				EstTrimLen += i;

				Table.PushTrimSample( EstTrimLen, GivenSizeAvailable.Height );
			}

			t.Text = t.Text.Substring( EstTrimLen );

			return TrimmedText;
		}

		private TextBlock NewTextBlock( string Text )
		{
			TextBlock t = new TextBlock()
			{
				TextAlignment = TextAlignment.Center,
				TextWrapping = TextWrapping.Wrap,
				TextLineBounds = TextLineBounds.TrimToBaseline
			};

			double lh = -.5 * FontSize + LineHeight;

			t.FontSize = FontSize;

			// Squeeze all half-width character to second row
			t.CharacterSpacing = 1000;
			t.Width = 2 * FontSize;

			// Restore the original width using minus fontsize prop
			t.Margin = new Thickness( lh, 0, lh, 0 );

			t.Text = Text;
			return t;
		}

		private double BlockHeightOf( TextBlock t )
		{
			t.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
			t.Arrange( new Rect( new Point(), t.DesiredSize ) );

			return t.ActualHeight;
		}
	}
}