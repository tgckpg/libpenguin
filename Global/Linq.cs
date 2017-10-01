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

		public static T[] Flattern<T>( this IEnumerable<T> SourceList, Func<T,IEnumerable<T>> Children )
		{
			if ( SourceList.Count() == 0 ) return new T[ 0 ];

			List<T> Flatterned = new List<T>();

			foreach( T Item in SourceList )
			{
				Flatterned.Add( Item );
				IEnumerable<T> ListChildren = Children( Item );

				if( ListChildren != null )
					Flatterned.AddRange( ListChildren.Flattern( Children ) );
			}

			return Flatterned.ToArray();
		}
	}
}