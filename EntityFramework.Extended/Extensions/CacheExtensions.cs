using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EntityFramework.Caching;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

namespace EntityFramework.Extensions
{
    /// <summary>
    /// Extension methods for query cache.
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// Returns the result of the <paramref name="query"/>; if possible from the cache,
        /// otherwise the query is materialized and the result cached before being returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="cachePolicy">The cache policy for the query.</param>
        /// <param name="tags">The list of tags to use for cache expiration.</param>
        /// <returns>
        /// The result of the query.
        /// </returns>
        public static IEnumerable<TEntity> FromCache<TEntity>(this IQueryable<TEntity> query, CachePolicy cachePolicy = null, IEnumerable<string> tags = null)
            where TEntity : class
        {
            if (tags == null)
            {
                var tag = string.IsNullOrWhiteSpace(Helper.LoggedInTenantId) ? "Global" : Helper.LoggedInTenantId;
                tags = new List<string>() { tag };
            }
            string key = query.GetCacheKey();
            var cacheKey = new CacheKey(key,
                tags ?? Enumerable.Empty<string>());

            // allow override of CacheManager
            var manager = Locator.Current.Resolve<CacheManager>();

            var result = manager.GetOrAdd(
                cacheKey,
                k => query.AsNoTracking().ToList(),
                cachePolicy ?? CachePolicy.Default
            ) as IEnumerable<TEntity>;

            return result;
        }
        /// <summary>
        /// Compile Rules against Single Entity
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="cachePolicy"></param>
        /// <param name="tags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<CompiledRule<T>> CompileSingleFromCache<T>(this IEnumerable<Rule> expression, CachePolicy cachePolicy = null, IEnumerable<string> tags = null) where T : class, new()
        {
            if (tags == null)
            {
                var tag = string.IsNullOrWhiteSpace(Helper.LoggedInTenantId) ? "Global" : Helper.LoggedInTenantId;
                tags = new List<string>() { tag };
            }
            //return key;
            var cacheKeyProvider = Locator.Current.Resolve<ICacheKeyProvider>();
            var rules = expression.ToList();
            // the key is potentially very long
            // create key based on cachekeyprovider
            string key = cacheKeyProvider.CreateKey(rules.FirstOrDefault()?.RuleKey + $"_CompileSingleFromCache");
            var cacheKey = new CacheKey(key,
                tags ?? Enumerable.Empty<string>());

            // allow override of CacheManager
            var manager = Locator.Current.Resolve<CacheManager>();
            var result = manager.GetOrAdd(
                cacheKey,
                k => CompileRule(new T(), rules),
                cachePolicy ?? CachePolicy.Default
            );
            return result as List<CompiledRule<T>>;
        }


        /// <summary>
        /// Comple Against Single Entity
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="rules"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<CompiledRule<T>> CompileRule<T>(T targetEntity, List<Rule> rules) where T : class
        {
            var compiledRules=new List<CompiledRule<T>>();
            foreach (var rule in rules)
            {
                var compile=new CompiledRule<T>(){Rule = rule};
                if (string.IsNullOrWhiteSpace(rule.ValidationDefination))
                {
                    compile.IsValid=arg => true;
                }
                else
                {
                    try
                    {
                        var lambda = System.Linq.Dynamic.DynamicExpression.ParseLambda<T, bool>(rule.ValidationDefination, null);
                        compile.IsValid = lambda.Compile();
                    }
                    catch (Exception e)
                    {
                        compile.IsValid = arg => false;
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(rule.ValidationDefination))
                {
                    try
                    {
                        ParameterExpression x = Expression.Parameter(typeof(T), "x");
                        var body = System.Linq.Dynamic.DynamicExpression.ParseLambda(new ParameterExpression[] { x }, null, rule.AssignmentDefination);
                        compile.ApplyLogic = body.Compile();
                    }
                    catch (Exception e)
                    {
                        throw new BusinessException(ErrorCode.GLB106,$"Custom Business Logic Failed to run. with Error Message:{e.GetBaseException().Message}");
                    }
                }
                compiledRules.Add(compile);
            }

            return compiledRules;
        }
#if NET45
        /// <summary>
        /// Returns the result of the <paramref name="query"/>; if possible from the cache,
        /// otherwise the query is materialized asynchronously and the result cached before being returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="cachePolicy">The cache policy for the query.</param>
        /// <param name="tags">The list of tags to use for cache expiration.</param>
        /// <returns>
        /// The result of the query.
        /// </returns>
        public static async Task<IEnumerable<TEntity>> FromCacheAsync<TEntity>(this IQueryable<TEntity> query, CachePolicy cachePolicy = null, IEnumerable<string> tags = null)
            where TEntity : class
        {
            if (tags == null)
            {
                var tag = string.IsNullOrWhiteSpace(Helper.LoggedInTenantId) ? "Global" : Helper.LoggedInTenantId;
                tags = new List<string>() { tag };
            }
            string key = query.GetCacheKey();
            var cacheKey = new CacheKey(key,
                tags ?? Enumerable.Empty<string>());

            // allow override of CacheManager
            var manager = Locator.Current.Resolve<CacheManager>();

            var result = await manager
                .GetOrAddAsync(
                    cacheKey,
                    async k => await query.AsNoTracking().ToListAsync().ConfigureAwait(false),
                    cachePolicy ?? CachePolicy.Default
                )
                .ConfigureAwait(false) as IEnumerable<TEntity>;

            return result;
        }

#endif

        /// <summary>
        /// Returns the first element of the <paramref name="query"/>; if possible from the cache,
        /// otherwise the query is materialized and the result cached before being returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="cachePolicy">The cache policy for the query.</param>
        /// <param name="tags">The list of tags to use for cache expiration.</param>
        /// <returns>default(T) if source is empty; otherwise, the first element in source.</returns>
        public static TEntity FromCacheFirstOrDefault<TEntity>(this IQueryable<TEntity> query, CachePolicy cachePolicy = null, IEnumerable<string> tags = null)
            where TEntity : class
        {
            if (tags == null)
            {
                var tag = string.IsNullOrWhiteSpace(Helper.LoggedInTenantId) ? "Global" : Helper.LoggedInTenantId;
                tags = new List<string>() { tag };
            }
            return query
                .Take(1)
                .FromCache(cachePolicy, tags)
                .FirstOrDefault();
        }

#if NET45
        /// <summary>
        /// Returns the first element of the <paramref name="query"/>; if possible from the cache,
        /// otherwise the query is materialized asynchronously and the result cached before being returned.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="cachePolicy">The cache policy for the query.</param>
        /// <param name="tags">The list of tags to use for cache expiration.</param>
        /// <returns>default(T) if source is empty; otherwise, the first element in source.</returns>
        public static async Task<TEntity> FromCacheFirstOrDefaultAsync<TEntity>(this IQueryable<TEntity> query, CachePolicy cachePolicy = null, IEnumerable<string> tags = null)
            where TEntity : class
        {
            if (tags == null)
            {
                var tag = string.IsNullOrWhiteSpace(Helper.LoggedInTenantId) ? "Global" : Helper.LoggedInTenantId;
                tags = new List<string>() { tag };
            }
            var q = await query
                .Take(1)
                .FromCacheAsync(cachePolicy, tags)
                .ConfigureAwait(false);

            return q.FirstOrDefault();
        }

#endif

        /// <summary>
        /// Removes the cached query from cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <returns>
        /// The original <paramref name="query"/> for fluent chaining.
        /// </returns>
        public static IQueryable<TEntity> RemoveCache<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            IEnumerable<TEntity> removed;
            return RemoveCache(query, out removed);
        }

        /// <summary>
        /// Removes the cached query from cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
        /// <param name="query">The query to be materialized.</param>
        /// <param name="removed">The removed items for cache.</param>
        /// <returns>
        /// The original <paramref name="query"/> for fluent chaining.
        /// </returns>
        public static IQueryable<TEntity> RemoveCache<TEntity>(this IQueryable<TEntity> query, out IEnumerable<TEntity> removed)
            where TEntity : class
        {
            string key = query.GetCacheKey();

            // allow override of CacheManager
            var manager = Locator.Current.Resolve<CacheManager>();

            removed = manager.Remove(key) as IEnumerable<TEntity>;
            return query;
        }
    }
}
