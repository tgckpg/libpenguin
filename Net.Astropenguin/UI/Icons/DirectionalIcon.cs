using System;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Net.Astropenguin.UI.Icons
{
    public enum Direction
    {
        Normal = 0
        , Rotate90 = 1
        , Rotate180 = 2
        , Rotate270 = 4
        , MirrorVertical = 8
        , MirrorHorizontal = 16
        , MirrorBoth = 32
    }

    public abstract class DirectionalIcon : IconBase
    {
        public static DependencyProperty DirectionProperty = DependencyProperty.Register( "Direction", typeof( Direction ), typeof( DirectionalIcon ), new PropertyMetadata( Direction.Normal, DirectionChanged ) );

        public Direction Direction
        {
            get { return ( Direction ) GetValue( DirectionProperty ); }
            set { SetValue( DirectionProperty, value ); }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DirectionPass();
        }

        protected override void ScaleStage( Size availableSize )
        {
            base.ScaleStage( availableSize );
            DirectionPass();
        }

        protected void DirectionPass()
        {
            if ( ( Direction & Direction.MirrorVertical ) != 0 )
            {
                StageTransform.ScaleX *= -1;
            }
            else if ( ( Direction & Direction.MirrorHorizontal ) != 0 )
            {
                StageTransform.ScaleY *= -1;
            }
            else if ( ( Direction & Direction.MirrorBoth ) != 0 )
            {
                StageTransform.ScaleX *= -1;
                StageTransform.ScaleY *= -1;
            }

            if ( ( Direction & Direction.Rotate90 ) != 0 )
            {
                StageTransform.Rotation = 90;
            }
            else if ( ( Direction & Direction.Rotate180 ) != 0 )
            {
                StageTransform.Rotation = 180;
            }
            else if ( ( Direction & Direction.Rotate270 ) != 0 )
            {
                StageTransform.Rotation = 270;
            }
        }

        private static void DirectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( d as DirectionalIcon ).DirectionPass();
        }

    }
}