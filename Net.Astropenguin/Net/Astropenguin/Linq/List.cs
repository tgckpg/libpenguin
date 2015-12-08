using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.Linq
{
    public static class List
    {
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
    }
}
