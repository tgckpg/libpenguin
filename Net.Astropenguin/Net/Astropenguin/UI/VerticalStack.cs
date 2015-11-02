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

        public const string StageName = "Stage";

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( VerticalStack ), new PropertyMetadata( "", VisualPropertyChanged ) );
        public string Text {
            get { return GetValue( TextProperty ) as string; }
            set { SetValue( TextProperty, value ); }
        }

        new public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register( "FontSize", typeof( double ), typeof( VerticalStack ), new PropertyMetadata( 16.0, VisualPropertyChanged ) );
        new public double FontSize {
            get
            {
                double? v = GetValue( FontSizeProperty ) as double?;
                if ( v == null ) return 16;
                return ( double ) v;
            }
            set { SetValue( FontSizeProperty, value ); }
        }

        public static readonly DependencyProperty LineHeightProperty = DependencyProperty.Register( "LineHeight", typeof( double ), typeof( VerticalStack ), new PropertyMetadata( 16.0, VisualPropertyChanged ) );
        public double LineHeight {
            get
            {
                double? v = GetValue( LineHeightProperty ) as double?;
                if ( v == null ) return 16;
                return ( double ) v;
            }
            set { SetValue( LineHeightProperty, value ); }
        }

        protected Size GivenSizeAvailable { get; private set; }

        private static void VisualPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( d as VerticalStack ).UpdateDisplay();
        }

        private StackPanel Stage;

        public VerticalStack()
            :base()
        {
            DefaultStyleKey = typeof( VerticalStack );
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Stage = GetTemplateChild( StageName ) as StackPanel;
            UpdateDisplay();
        }

        protected override Size MeasureOverride( Size availableSize )
        {
            // If the Size Changes, we need to update the text
            // TODO: Use a more efficient approach
            /* if( !GivenSizeAvailable.Equals( availableSize ) )
            {
                UpdateDisplay( availableSize );
            }
            */

            // Get the available size from parent
            GivenSizeAvailable = new Size( availableSize.Width, availableSize.Height );

            return base.MeasureOverride( availableSize );
        }

        private void UpdateDisplay()
        {
            ClearStage();
            // I can't draw nothing, so... remove everything?
            if ( GivenSizeAvailable.Height == 0 || Text == "" ) return;

            DrawTextBlocks( Text );
        }

        private void UpdateDisplay( Size availableSize )
        {
            if ( availableSize.Height == GivenSizeAvailable.Height )
            {
                // If height is equal, that means the width is changed
                // So we either need to do nothing, or remove children from Stage

            }
            // If the height is changed, we need to redo the drawings
            else UpdateDisplay();
        }

        private void ClearStage()
        {
            if ( Stage == null ) return;
            while ( 0 < Stage.Children.Count() )
                Stage.Children.RemoveAt( 0 );
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

            // Does the MasterTextBlock still have some text?
            if ( 0 < t.Text.Length )
            {
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

            BlockHeightOf( t );

            int EstTrimmingLength = ( int ) Math.Floor( t.Text.Length * GivenSizeAvailable.Height / t.ActualHeight );

            if ( t.Text.Length < EstTrimmingLength || t.Text == "" ) return null;

            TextBlock TrimmedText = NewTextBlock(
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

            t.Text = t.Text.Substring( EstTrimmingLength + i );

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
            t.Margin = new Thickness( lh , 0, lh, 0 );

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
