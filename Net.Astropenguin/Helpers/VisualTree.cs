using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Net.Astropenguin.Helpers
{
	public class VisualTree
	{
		public static T At_0<T>( UIElement p, uint l )
		{
			if ( VisualTreeHelper.GetChildrenCount( p ) > 0 )
			{
				DependencyObject k = VisualTreeHelper.GetChild( p, 0 );

				if ( k is UIElement && l > 0 )
					return At_0<T>( ( UIElement ) k, l - 1 );

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

			if ( p is T )
			{
				return ( T ) ( object ) p;
			}

			return default( T );
		}
	}

	public static class UIElementExt
	{
		/// <summary>
		/// Drill down the first element n + 1 times
		/// i.e. P(4) e[0][0][0][0][0]
		/// </summary>
		public static T Child_0<T>( this UIElement e, uint n )
		{
			return VisualTree.At_0<T>( e, n );
		}

		public static T ChildAt<T>( this UIElement e, params uint[] l )
		{
			return VisualTree.At<T>( e, l );
		}
	}
}