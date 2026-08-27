using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Caching;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.Infrastructure.Services
{
    public class TrackoCacheKeyProvider:ICacheKeyProvider
    {
        /// <summary>
        /// Creates the cache key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string CreateKey(string value)
        {
            var newvalue = $"{Helper.LoggedInTenantId}-{value}";
            var bytes = Encoding.Unicode.GetBytes(newvalue.ToCharArray());
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);

            // concat the hash bytes into one long string
            return hash.Aggregate(new StringBuilder(32),
                    (sb, b) => sb.Append(b.ToString("X2")))
                .ToString();
        }
    }
}
