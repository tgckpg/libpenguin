using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Net.Astropenguin.Helpers
{
	public class VisualTree
	{
		public static T At<T>( UIElement p, uint l )
		{
			if ( VisualTreeHelper.GetChildrenCount( p ) > 0 )
			{
				DependencyObject k = VisualTreeHelper.GetChild( p, 0 );

				if ( k is UIElement && l > 0 )
					return At<T>( ( UIElement ) k, l - 1 );

				if ( k is T )
				{
					return ( T ) ( object ) k;
				}
			}

			return default( T );
		}

		public static T At<T>( UIElement p, uint[] l )
		{
			foreach ( int i in l )
			{
				if ( i < VisualTreeHelper.GetChildrenCount( p ) )
				{
					p = ( UIElement ) VisualTreeHelper.GetChild( p, i );
				}
			}

			if( p is T )
			{
				return ( T ) ( object ) p;
			}

			return default( T );
		}
	}

	public static class UIElementExt
	{
		public static T ChildAt<T>( this UIElement e, uint l )
		{
			return VisualTree.At<T>( e, l );
		}

		public static T ChildAt<T>( this UIElement e, params uint[] l )
		{
			return VisualTree.At<T>( e, l );
		}
	}
}
