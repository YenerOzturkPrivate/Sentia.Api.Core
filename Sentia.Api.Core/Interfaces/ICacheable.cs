using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Interfaces
{
    public interface ICacheable
    {
        CacheOption CacheOption { get; }
        KeyValuePair<object[], TimeSpan> CacheSettings { get; }
    }

    public interface INoCache
    {
        bool NoCache { get; set; }
    }

    public enum CacheOption
    {
        None = 0,
        Memory = 1,
        Distributed = 2,
        Multi = 3
    }
}
