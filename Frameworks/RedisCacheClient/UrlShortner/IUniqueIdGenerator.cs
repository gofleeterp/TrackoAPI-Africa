using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisCacheClient
{
    public interface IUniqueIdGenerator
    {
        Task<long> GetNext();
    }
}
