using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Net.Astropenguin.Loaders
{
	public interface ILoader<T>
	{
		/// <summary>
		/// The Connector object is used by Observables to indicate the results are loaded
		/// </summary>
		Action<IList<T>> Connector { get; set; }
		int CurrentPage { get; }
		bool PageEnded { get; }
		Task<IList<T>> NextPage( uint count );
	}
}
