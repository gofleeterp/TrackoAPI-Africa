using RedisCacheClient.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisCacheClient
{
    interface IRedisCacheConnectionPoolManager
    {
        /// <summary>
        /// Get the Redis connection
        /// </summary>
        /// <returns>Returns an instance of<see cref="IConnectionMultiplexer"/>.</returns>
        IConnectionMultiplexer GetConnection();

        /// <summary>
        ///     Gets the information about the connection pool
        /// </summary>
        ConnectionPoolInformation GetConnectionInformations();
    }
}
