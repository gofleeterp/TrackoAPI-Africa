using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedisCacheClient.UrlShortner
{
    public class UrlRepository : IUrlRepository
    {
        private readonly IDatabase database;

        public UrlRepository(IDatabase database)
        {
            this.database = database;
        }

        public Task<bool> SaveUrl(long id, string url,TimeSpan? expirySpan)
        {
            if(!expirySpan.HasValue)
            {
                expirySpan = TimeSpan.FromDays(2);
            }
            return this.database.StringSetAsync(id.ToString(), url, expirySpan);
        }

        public async Task<string> GetUrl(long id)
        {
            var value = await this.database.StringGetAsync(id.ToString());
            return value;
        }
    }
}
