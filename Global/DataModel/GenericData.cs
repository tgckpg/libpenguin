using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Astropenguin.DataModel
{
	public class GenericData<T>
	{
		public T Data { get; private set; }

		public static IEnumerable<GenericData<T>> Convert( IEnumerable<T> Data )
		{
			List<GenericData<T>> GenericList = new List<GenericData<T>>();
			foreach ( T Item in Data )
			{
				GenericList.Add( new GenericData<T>() { Data = Item } );
			}

			return GenericList;
		}

		public static IEnumerable<GenericData<T>> Convert( Array Data )
		{
			List<GenericData<T>> GenericList = new List<GenericData<T>>();
			foreach ( T Item in Data )
			{
				GenericList.Add( new GenericData<T>() { Data = Item } );
			}

			return GenericList;
		}
	}
}
