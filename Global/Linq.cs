using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.Linq
{
	public static class LinqExtensions
	{
		public static TTarget[] Remap<TSource, TTarget>( this IEnumerable<TSource> Source, Func<TSource, TTarget> Translator )
		{
			int i = 0; int l = Source.Count();

			TTarget[] Translated = new TTarget[ l ];
			foreach ( TSource Item in Source )
			{
				Translated[ i++ ] = Translator( Item );
			}

			return Translated;
		}

		public static async Task<TTarget[]> Remap<TSource, TTarget>( this IEnumerable<TSource> Source, Func<TSource, Task<TTarget>> Translator )
		{
			int i = 0; int l = Source.Count();

			TTarget[] Translated = new TTarget[ l ];
			foreach ( TSource Item in Source )
			{
				Translated[ i++ ] = await Translator( Item );
			}

			return Translated;
		}

		public static T[] Where<T>( this IEnumerable<T> Source, Func<int, T, bool> Translator )
		{
			int i = 0; int l = Source.Count();

			List<T> Translated = new List<T>();
			foreach ( T Item in Source )
			{
				if ( Translator( i++, Item ) )
				{
					Translated.Add( Item );
				}
			}

			return Translated.ToArray();
		}

		public static TTarget[] Remap<TSource, TTarget>( this IEnumerable<TSource> Source, Func<TSource, int, TTarget> Translator )
		{
			int i = 0; int l = Source.Count();

			TTarget[] Translated = new TTarget[ l ];
			foreach ( TSource Item in Source )
			{
				Translated[ i ] = Translator( Item, i );
				i++;
			}

			return Translated;
		}

		public static TProject[] Remap<TSource, TProject>( this TSource Target, int i, int l, Func<TSource, int, TProject> Project )
		{
			TProject[] Result = new TProject[ l ];
			for ( ; i < l; i++ ) Result[ i ] = Project( Target, i );
			return Result;
		}

		public static void AggExec<T>( this IEnumerable<T> Items, Action<T, T, byte> Agg )
		{
			IEnumerator<T> E = Items.GetEnumerator();

			T T1, T2;

			if ( E.MoveNext() )
			{
				T1 = E.Current;
				Agg( default( T ), T1, 0 );
			}
			else
			{
				return;
			}

			while ( E.MoveNext() )
			{
				T2 = E.Current;
				Agg( T1, T2, 1 );
				T1 = T2;
			}

			Agg( T1, default( T ), 2 );
		}

		public static bool AggAny<T>( this IEnumerable<T> Items, Func<T, T, byte, bool> Agg )
		{
			IEnumerator<T> E = Items.GetEnumerator();

			T T1, T2;

			if ( E.MoveNext() )
			{
				T1 = E.Current;
				if( Agg( default( T ), T1, 0 ) )
				{
					return true;
				}
			}
			else
			{
				return false;
			}

			while ( E.MoveNext() )
			{
				T2 = E.Current;
				if( Agg( T1, T2, 1 ) )
				{
					return true;
				}
				T1 = T2;
			}

			return Agg( T1, default( T ), 2 );
		}

		public static void ExecEach<T>( this IEnumerable<T> Items, Action<T> A )
		{
			foreach ( T Item in Items ) A( Item );
		}

		public static void ExecEach<T>( this IEnumerable<T> Items, Action<T, int> A )
		{
			int i = 0;
			foreach ( T Item in Items ) A( Item, i++ );
		}

		public static async Task ExecEach<T>( this IEnumerable<T> Items, Func<T, Task> A )
		{
			foreach ( T Item in Items ) await A( Item );
		}

		public static async Task ExecEach<T>( this IEnumerable<T> Items, Func<T, int, Task> A )
		{
			int i = 0;
			foreach ( T Item in Items ) await A( Item, i++ );
		}

		public static void Filter<TSource>( this IList<TSource> source, Func<TSource, bool> keySelector )
		{
			TSource[] FilteredList = source.Where( keySelector ).ToArray();

			source.Clear();

			if( FilteredList != null )
			foreach ( TSource Item in FilteredList )
			{
				source.Add( Item );
			}
		}

		public static void Sort<TSource, TKey>( this IList<TSource> source, Func<TSource, TKey> keySelector )
		{
			TSource[] sortedList = source.OrderBy( keySelector ).ToArray();
			source.Clear();
			foreach ( TSource Item in sortedList )
			{
				source.Add( Item );
			}
		}

		public static void SortDesc<TSource, TKey>( this IList<TSource> source, Func<TSource, TKey> keySelector )
		{
			TSource[] sortedList = source.OrderByDescending( keySelector ).ToArray();
			source.Clear();
			foreach ( TSource Item in sortedList )
			{
				source.Add( Item );
			}
		}

		/// <summary>
		/// Draw each element using Yates-Shuffle
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="SourceList">The source list</param>
		/// <param name="Draw">The draw action</param>
		/// <param name="Rand">Function that picks a random choice for given range</param>
		public static void DrawEach<T>( this IList<T> SourceList, Action<T, int, int> Draw, Func<int,int,int> Rand )
		{
			T[] SList = SourceList.ToArray();
			int l = SourceList.Count();

			for ( int i = 0; i < l; i++ )
			{
				int Choice = Rand( i, l );
				T Item = SList[ Choice ];

				SList[ Choice ] = SList[ i ];

				Draw( Item, i, Choice );
			}
		}

		/// <summary>
		/// Breakdown the T into smaller components of T with a series of breakers
		/// </summary>
		public static T[] Breakdown<T>( this IEnumerable<T> SourceList, params Func<T, IEnumerable<T>>[] Breakers )
		{
			if ( SourceList.Count() == 0 )
				return new T[ 0 ];

			List<T> BrokenDown = new List<T>();
			Breakdown( BrokenDown, SourceList, Breakers );
			return BrokenDown.ToArray();
		}

		private static void Breakdown<T>( IList<T> Container, IEnumerable<T> SourceList, IEnumerable<Func<T, IEnumerable<T>>> Breakers )
		{
			if ( SourceList == null )
				return;

			Func<T, IEnumerable<T>> Breaker = Breakers.FirstOrDefault();
			if( Breaker == null )
			{
				foreach ( T Item in SourceList )
					Container.Add( Item );
			}
			else
			{
				foreach ( T Item in SourceList )
				{
					Breakdown( Container, Breaker( Item ), Breakers.Skip( 1 ) );
				}
			}
		}

		public static T[] Flattern<T>( this IEnumerable<T> SourceList, Func<T, IEnumerable<T>> Children, int Depth = 0 )
		{
			if ( SourceList.Count() == 0 )
				return new T[ 0 ];

			List<T> Flatterned = new List<T>();
			Flattern( Flatterned, SourceList, Children, 0, Depth );
			return Flatterned.ToArray();
		}

		private static void Flattern<T>( IList<T> Container, IEnumerable<T> SourceList, Func<T, IEnumerable<T>> Children, int Level, int Depth )
		{
			if ( SourceList == null )
				return;

			foreach ( T Item in SourceList )
			{
				Container.Add( Item );
				if ( Depth == 0 || Level < Depth )
				{
					Flattern( Container, Children( Item ), Children, Level + 1, Depth );
				}
			}
		}

	}
}