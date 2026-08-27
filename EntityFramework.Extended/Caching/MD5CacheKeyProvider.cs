using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;

namespace EntityFramework.Caching
{
    /// <summary>
    /// MD5 Cache Key Provider
    /// </summary>
    public class MD5CacheKeyProvider:ICacheKeyProvider
    {
        /// <summary>
        /// Creates the cache key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string CreateKey_Old(string value)
        {
            var bytes = Encoding.Unicode.GetBytes(value.ToCharArray());
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);

            // concat the hash bytes into one long string
            return hash.Aggregate(new StringBuilder(32),
                    (sb, b) => sb.Append(b.ToString("X2")))
                .ToString();
        }
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
