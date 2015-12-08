using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Net.Astropenguin.Logging;

namespace Net.Astropenguin.UI
{
    [TemplatePart( Name = StageName, Type = typeof( StackPanel ))]
    public class VerticalStack : Control
    {
        public static readonly string ID = typeof( VerticalStack ).Name;
        private static readonly Size SIZE_NULL = new Size( 0, 0 );

        public const string StageName = "Stage";

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( VerticalStack ), new PropertyMetadata( "", VisualPropertyChanged ) );
        public string Text {
            get { return GetValue( TextProperty ) as string; }
            set { SetValue( TextProperty, value ); }
        }

        new public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register( "FontSize", typeof( double ), typeof( VerticalStack ), new PropertyMetadata( 0.0, VisualPropertyChanged ) );
        new public double FontSize {
            get
            {
                return ( double ) GetValue( FontSizeProperty );
            }
            set { SetValue( FontSizeProperty, value ); }
        }

        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register( "LineHeight", typeof( double ), typeof( VerticalStack ), new PropertyMetadata( 16.0, LineHeightChanged ) );
        public double LineHeight {
            get
            {
                return ( double ) GetValue( LineHeightProperty );
            }
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
            :base()
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
            if( GivenSizeAvailable.Equals( SIZE_NULL ) && !GivenSizeAvailable.Equals( availableSize ) )
            {
                UpdateDisplay( availableSize );
            }

            // Get the available size from parent
            GivenSizeAvailable = new Size( availableSize.Width, availableSize.Height );

            return base.MeasureOverride( availableSize );
        }

        private void UpdateDisplay()
        {
            /* INTENSIVE_LOG
            Logger.Log( ID, "UpdateDisplay", LogType.DEBUG );
            //*/

            ClearStage();
            // I can't draw nothing, so... remove everything?
            if ( GivenSizeAvailable.Height == 0 || Text == "" || FontSize == 0 ) return;

            DrawTextBlocks( Text );
        }

        private void UpdateDisplay( Size availableSize )
        {
            if ( availableSize.Height == GivenSizeAvailable.Height )
            {
                // If height is equal, that means the width is changed
                // So we either need to do nothing, or remove children from Stage
                // if ( Width < FontSize ) ClearStage();
            }
            // If the height is changed, we need to redo the drawings
            else
            {
                GivenSizeAvailable = availableSize;
                UpdateDisplay();
            }
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
            if ( Stage == null || Stage.Children.Count == 0 ) return;

            foreach( TextBlock Tx in Stage.Children )
            {
                double lh = -.5 * FontSize + LineHeight;
                Tx.Margin = new Thickness( lh, 0, lh, 0 );
            }
        }

        private void DrawTextBlocks( string Text )
        {
            TextBlock t = NewTextBlock( Text );

            TextBlock Tx;
            while ( ( Tx = TrimText( t ) ) != null )
            {
                // Two special char to ensure the center alignment
                Tx.Text += "\u3000\u2007";
                Stage.Children.Add( Tx );
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
            if( double.IsInfinity( GivenSizeAvailable.Height ) )
            {
                return null;
            }

            TextBlock TrimmedText = null;

            int EstTrimmingLength = 0;

            VerticalLogaTable Table = VerticalLogaManager.GetLoga( FontSize );
            if( 5 < Table.CertaintyLevel )
            {
                EstTrimmingLength = Table.GetTrimLenForHeight( GivenSizeAvailable.Height );

                if ( EstTrimmingLength == 0 || t.Text.Length < EstTrimmingLength || t.Text == "" ) return null;

                TrimmedText = NewTextBlock(
                    t.Text.Substring( 0, EstTrimmingLength )
                );
            }
            else
            {
                BlockHeightOf( t );
                EstTrimmingLength = ( int ) Math.Floor( t.Text.Length * GivenSizeAvailable.Height / t.ActualHeight );

                if ( EstTrimmingLength == 0 || t.Text.Length < EstTrimmingLength || t.Text == "" ) return null;

                TrimmedText = NewTextBlock(
                    t.Text.Substring( 0, EstTrimmingLength )
                );

                // Add text one by one
                int i = 0;
                while ( BlockHeightOf( TrimmedText ) < GivenSizeAvailable.Height
                    && ( EstTrimmingLength + i ) < t.Text.Length
                )
                {
                    TrimmedText.Text += t.Text[ EstTrimmingLength + ( i ++ ) ];
                }

                // Then remove them one by one to get the best estimation
                while ( GivenSizeAvailable.Height < BlockHeightOf( TrimmedText ) )
                {
                    TrimmedText.Text = TrimmedText.Text.Substring( 0, EstTrimmingLength + ( -- i ) );
                }

                EstTrimmingLength += i;

                Table.PushTrimSample( EstTrimmingLength, GivenSizeAvailable.Height );
            }

            t.Text = t.Text.Substring( EstTrimmingLength );

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
            t.Width = 2*FontSize;

            // Restore the original width using minus fontsize prop
            t.Margin = new Thickness( lh, 0, lh, 0 );

            t.Text = Text;
            return t;
        }

        private double BlockHeightOf( TextBlock t )
        {
            t.Measure( new Size( double.PositiveInfinity, double.PositiveInfinity ) );
            t.Arrange( new Rect( new Point(), t.DesiredSize ) );

            /* INTENSIVE_LOG
            if( t.FontSize == 16 )
            {
                Logger.Log( ID, string.Format( "FontSize 16 detected {0}, Should be {1}. Text {2}", t.FontSize, FontSize, t.Text ), LogType.DEBUG );
            }
            //*/

            return t.ActualHeight;
        }
    }
}
