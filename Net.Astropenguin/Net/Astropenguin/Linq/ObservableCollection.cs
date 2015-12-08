using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.Linq
{
    public static class ObservableCollection
    {
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
