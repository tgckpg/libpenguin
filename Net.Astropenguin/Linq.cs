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
        public static IEnumerable<TTarget> Remap<TSource, TTarget>( this IEnumerable<TSource> Source, Func<TSource, TTarget> Translator )
        {
            int i = 0; int l = Source.Count();

            TTarget[] Translated = new TTarget[ l ];
            foreach ( TSource Item in Source )
            {
                Translated[ i++ ] = Translator( Item );
            }

            return Translated;
        }

        public static void Filter<TSource>( this List<TSource> source, Func<TSource, bool> keySelector )
        {
            List<TSource> FilteredList = source.Where( keySelector ).ToList();

            source.Clear();

            if( FilteredList != null )
            foreach ( TSource Item in FilteredList )
            {
                source.Add( Item );
            }
        }

        public static void Sort<TSource, TKey>( this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector )
        {
            List<TSource> sortedList = source.OrderBy( keySelector ).ToList();
            source.Clear();
            foreach ( TSource Item in sortedList )
            {
                source.Add( Item );
            }
        }

        public static void SortDesc<TSource, TKey>( this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector )
        {
            List<TSource> sortedList = source.OrderByDescending( keySelector ).ToList();
            source.Clear();
            foreach ( TSource Item in sortedList )
            {
                source.Add( Item );
            }
        }
    }
}
