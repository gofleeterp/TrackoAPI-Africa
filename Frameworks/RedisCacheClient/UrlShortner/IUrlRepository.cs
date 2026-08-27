using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisCacheClient.UrlShortner
{
    public interface IUrlRepository
    {
        Task<string> GetUrl(long id);
        Task<bool> SaveUrl(long id, string url, TimeSpan? expirySpan);
    }
}
