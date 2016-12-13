using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Net.Astropenguin.UI.Icons
{
    using Helpers;

    public class IconNavigateArrow : DirectionalIcon { public IconNavigateArrow() { DefaultStyleKey = typeof( IconNavigateArrow ); } }
    public class IconLogin : DirectionalIcon { public IconLogin() { DefaultStyleKey = typeof( IconLogin ); } }
    public class IconLogout : DirectionalIcon { public IconLogout() { DefaultStyleKey = typeof( IconLogout ); } }
    public class IconTOC : DirectionalIcon { public IconTOC() { DefaultStyleKey = typeof( IconTOC ); } }
    public class IconKey : DirectionalIcon { public IconKey() { DefaultStyleKey = typeof( IconKey ); } }
    public class IconPlusSign : IconBase { public IconPlusSign() { DefaultStyleKey = typeof( IconPlusSign ); } }
    public class IconBookmark : IconBase { public IconBookmark() { DefaultStyleKey = typeof( IconBookmark ); } }
    public class IconReload : IconBase { public IconReload() { DefaultStyleKey = typeof( IconReload ); } }
    public class IconSettings : IconBase { public IconSettings() { DefaultStyleKey = typeof( IconSettings ); } }
    public class IconExpand : IconBase { public IconExpand() { DefaultStyleKey = typeof( IconExpand ); } }
    public class IconRetract : IconBase { public IconRetract() { DefaultStyleKey = typeof( IconRetract ); } }
    public class IconStar : IconBase { public IconStar() { DefaultStyleKey = typeof( IconStar ); } }
    public class IconImage : IconBase { public IconImage() { DefaultStyleKey = typeof( IconImage ); } }

    public class IconCross : IconBase { public IconCross() { DefaultStyleKey = typeof( IconCross ); } }
    public class IconTick : IconBase { public IconTick() { DefaultStyleKey = typeof( IconTick ); } }

    public class IconUseInertia : IconBase { public IconUseInertia() { DefaultStyleKey = typeof( IconUseInertia ); } }
    public class IconNoInertia : IconBase { public IconNoInertia() { DefaultStyleKey = typeof( IconNoInertia ); } }

    public class IconInfo : IconBase { public IconInfo() { DefaultStyleKey = typeof( IconInfo ); } }
    public class IconPlay : IconBase { public IconPlay() { DefaultStyleKey = typeof( IconPlay ); } }
    public class IconSave : IconBase { public IconSave() { DefaultStyleKey = typeof( IconSave ); } }
    public class IconSearch : IconBase { public IconSearch() { DefaultStyleKey = typeof( IconSearch ); } }
    public class IconSpider : IconBase { public IconSpider() { DefaultStyleKey = typeof( IconSpider ); } }
    public class IconSteps : IconBase { public IconSteps() { DefaultStyleKey = typeof( IconSteps ); } }
    public class IconAtomic : IconBase { public IconAtomic() { DefaultStyleKey = typeof( IconAtomic ); } }
    public class IconEEye : IconBase { public IconEEye() { DefaultStyleKey = typeof( IconEEye ); } }
    public class IconScript : IconBase { public IconScript() { DefaultStyleKey = typeof( IconScript ); } }

    public class IconSerial : IconBase { public IconSerial() { DefaultStyleKey = typeof( IconSerial ); } }
    public class IconParallel : IconBase { public IconParallel() { DefaultStyleKey = typeof( IconParallel ); } }

    public class IconTestTube : IconBase { public IconTestTube() { DefaultStyleKey = typeof( IconTestTube ); } }

    public sealed class IconOneDrive : PathIcon
    {
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Path P = this.ChildAt<Path>( 0, 0 );
            P.SetBinding( Path.DataProperty, new Binding() { Source = @"
F0 M 0,0 C -1.61,-0.4 -2.5,-1.68 -2.5,-3.58 -2.5,-4.18 -2.46,-4.47 -2.31,-4.86
 -1.94,-5.82 -0.96,-6.54 0.33,-6.8 0.97,-6.93 1.17,-7.08 1.17,-7.4 1.17,-7.51 1.24,-7.81 1.34,-8.09
 1.76,-9.32 2.55,-10.35 3.39,-10.77 4.26,-11.21 4.71,-11.31 5.77,-11.3 7.27,-11.3 8.03,-10.97 9.08,-9.86
L 9.66,-9.25 10.18,-9.43 C 12.68,-10.3 15.19,-8.82 15.39,-6.35 L 15.44,-5.67 15.94,-5.49
C 17.35,-4.99 18.01,-3.92 17.89,-2.36 17.81,-1.34 17.33,-0.52 16.58,-0.11 L 16.22,0.08
 8.33,0.09 C 2.26,0.1 0.33,0.08 0,0 z
M -5.92,-1.1 C -6.86,-1.32 -7.85,-2.15 -8.32,-3.09 -8.58,-3.63 -8.59,-3.71 -8.59,-4.66
 -8.59,-5.57 -8.57,-5.71 -8.36,-6.16 -7.92,-7.11 -7.07,-7.79 -6.01,-8.06 -5.78,-8.12 -5.57,-8.21 -5.54,-8.26
 -5.5,-8.31 -5.47,-8.61 -5.45,-8.91 -5.38,-10.81 -4.14,-12.47 -2.41,-13.01 -1.47,-13.3 -0.3,-13.23 0.72,-12.82
 1.04,-12.69 1.01,-12.66 1.69,-13.56 2.09,-14.09 2.91,-14.75 3.58,-15.09 4.3,-15.45 5.04,-15.62 5.94,-15.61
 8.44,-15.61 10.6,-14.04 11.4,-11.64 11.66,-10.88 11.64,-10.66 11.34,-10.66 11.21,-10.65 10.83,-10.58 10.51,-10.5
L 9.91,-10.34 9.36,-10.89 C 7.83,-12.42 5.32,-12.76 3.19,-11.71 2.34,-11.28 1.66,-10.68 1.14,-9.89
 0.77,-9.33 0.3,-8.29 0.3,-8.03 0.3,-7.84 0.15,-7.75 -0.48,-7.54 -2.45,-6.9 -3.6,-5.4 -3.6,-3.49
 -3.6,-2.79 -3.42,-1.94 -3.17,-1.45 -3.08,-1.27 -3.03,-1.1 -3.06,-1.07 -3.13,-0.99 -5.58,-1.01 -5.92,-1.1 z"
            } );

            MatrixTransform Trans = new MatrixTransform();
            Trans.Matrix = new Matrix( 1, 0, 0, 1, 30.29, 18.4 );

            P.RenderTransform = Trans;
        }
    }

    public sealed class IconBing : PathIcon
    {
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Path P = this.ChildAt<Path>( 0, 0 );
            P.SetBinding(
                Path.DataProperty
                , new Binding() { Source = @"M 0,0 L 3.97,1.39 3.97,15.36 9.56,12.14 6.82,10.85 5.09,6.55 13.89,9.64 13.89,14.14 3.97,19.87 0,17.66 Z" }
            );
            P.VerticalAlignment = VerticalAlignment.Center;
            P.HorizontalAlignment = HorizontalAlignment.Center;

            P.Width = 14;
            P.Height = 20;
        }
    }
}