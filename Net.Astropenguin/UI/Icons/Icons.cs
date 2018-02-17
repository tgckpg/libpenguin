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
	public class IconSteps : IconBase { public IconSteps() { DefaultStyleKey = typeof( IconSteps ); } }
	public class IconAtomic : IconBase { public IconAtomic() { DefaultStyleKey = typeof( IconAtomic ); } }
	public class IconEEye : IconBase { public IconEEye() { DefaultStyleKey = typeof( IconEEye ); } }
	public class IconScript : IconBase { public IconScript() { DefaultStyleKey = typeof( IconScript ); } }

	public class IconSerial : IconBase { public IconSerial() { DefaultStyleKey = typeof( IconSerial ); } }
	public class IconParallel : IconBase { public IconParallel() { DefaultStyleKey = typeof( IconParallel ); } }

	public class IconTestTube : IconBase { public IconTestTube() { DefaultStyleKey = typeof( IconTestTube ); } }

	public sealed class IconBing : PathIcon
	{
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			Path P = this.ChildAt<Path>( 0, 0 );
			P.SetBinding(
				Path.DataProperty
				, new Binding() { Source = "M 0,0 L 3.97,1.39 3.97,15.36 9.56,12.14 6.82,10.85 5.09,6.55 13.89,9.64 13.89,14.14 3.97,19.87 0,17.66 Z" }
			);
			P.VerticalAlignment = VerticalAlignment.Center;
			P.HorizontalAlignment = HorizontalAlignment.Center;

			P.Width = 14;
			P.Height = 20;
		}
	}
}