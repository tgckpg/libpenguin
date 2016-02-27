using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Net.Astropenguin.Loaders
{
    public interface ILoader<T>
    {
        Action<IList<T>> Connector { get; set; }
        int CurrentPage { get; }
        bool PageEnded { get; }
        Task<IList<T>> NextPage( uint count );
    }
}
