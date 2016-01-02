using System;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace Net.Astropenguin.UI.Icons
{
    public enum Direction
    {
        Normal
        , Rotate90, Rotate180, Rotate270
        , MirrorHorizontal, MirrorVertical, MirrorBoth
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
            switch( Direction )
            {
                case Direction.MirrorHorizontal:
                    StageTransform.ScaleX *= -1;
                    break;
                case Direction.Rotate180:
                case Direction.MirrorVertical:
                    StageTransform.ScaleY *= -1;
                    break;
                case Direction.MirrorBoth:
                    StageTransform.ScaleX *= -1;
                    StageTransform.ScaleY *= -1;
                    break;
                case Direction.Rotate90:
                    StageTransform.Rotation = 90;
                    break;
                case Direction.Rotate270:
                    StageTransform.Rotation = 270;
                    break;
            }
        }

        private static void DirectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( d as DirectionalIcon ).DirectionPass();
        }
    }

}
