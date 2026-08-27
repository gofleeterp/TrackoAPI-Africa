using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;

namespace TrackoApi.Core.Rules
{
    public static class RulesExtenation
    {
        private static readonly Dictionary<string, object> Symbols = new Dictionary<string, object>();
        private static MemoryCache Cache => MemoryCache.Default;
        private static readonly CacheItemPolicy DefaultPolicy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromHours(3) };
        public static void ClearCache()
        {
            try
            {
                Cache.Trim(100);
            }
            catch { }
        }
        public static bool Validate<T, TContext>(this T parameter, TContext ctx, string predicate, long ruleid = 0) where T : class
        {
            try
            {
                var t1 = parameter.GetType();
                var t2 = ctx.GetType();
                var cacheKey = $"{Helpers.Helper.LoggedInTenantId}_{(ruleid > 0 ? $"{ruleid}" : $"{t1.Name}_{predicate}")}";
                return ValidateCore<T, TContext>(t1,t2,predicate, cacheKey)(parameter, ctx);
            }
            catch (Exception e)
            {
                throw;
            }

        }
        public static bool Validate<T>(this T parameter, string predicate, long ruleid = 0) where T : class
        {
            try
            {
                var t1 = parameter.GetType();
                var cacheKey = $"{Helpers.Helper.LoggedInTenantId}_{(ruleid > 0 ? $"{ruleid}" : $"{t1.Name}_{predicate}")}";
                return ValidateCore<T>(t1, predicate, cacheKey)(parameter);
            }
            catch (Exception e)
            {
                throw;
            }

        }
        private static Func<T, TContext, bool> ValidateCore<T, TContext>(Type t1,Type t2,string predicate, string cacheKey) where T : class
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Func<T, TContext, bool> fun) return fun;
            }
            Expression<Func<T, TContext, bool>> lambda = (Expression<Func<T, TContext, bool>>)GofDynamicExpression.ParseLambda(new ParameterExpression[] { BP(t1), BP(t1) }, typeof(bool),
                predicate, Symbols);
            var compiled = lambda.Compile();
            Cache.Add(cacheKey, compiled, DefaultPolicy);
            return compiled;
        }
        private static Func<T, bool> ValidateCore<T>(Type t1,string predicate,string cacheKey) where T : class
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Func<T, bool> fun) return fun;
            }
            Expression<Func<T, bool>> lambda = (Expression<Func<T, bool>>)GofDynamicExpression.ParseLambda(new ParameterExpression[] { BP(t1) }, typeof(bool),
                predicate, Symbols);
            var compiled = lambda.Compile();
            Cache.Add(cacheKey, compiled, DefaultPolicy);
            return compiled;
        }

        public static void ApplyRule<T, TContext>(this T parameter, TContext ctx, string expression, long ruleid = 0)
            where T : class
            where TContext : class
        {
            var t1 = parameter.GetType();
            var t2 = ctx.GetType();
            var statements = expression.Split(';');
            foreach (var statement in statements)
            {
                try
                {
                    var cacheKey = $"{Helpers.Helper.LoggedInTenantId}_{(ruleid > 0 ? $"{ruleid}" : $"{t1.Name}_{statement}")}";
                    ApplyRuleCore(t1,t2, statement, cacheKey).DynamicInvoke(parameter, ctx);
                }
                catch (Exception ex)
                {
                    throw;

                }
            }


        }
        private static Delegate ApplyRuleCore(Type t1,Type t2,string expression, string cacheKey)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Delegate fun)
                {
                    return fun;
                }
            }
            var body = GofDynamicExpression.ParseLambda(new[] { BP(t1), BP(t2) }, null, expression, Symbols);
            var compiled = body.Compile();
            Cache.Add(cacheKey, compiled,DefaultPolicy);
            return compiled;
        }

        public static void ApplyRule<T>(this T parameter, string expression, long ruleid = 0) where T : class
        {
            var statements = expression.Split(';');
            var p1 = parameter.GetType();
            foreach (var statement in statements)
            {
                try
                {                    
                    var cacheKey = $"{Helpers.Helper.LoggedInTenantId}_{(ruleid > 0 ? $"{ruleid}" : $"{p1.Name}_{statement}")}";
                    ApplyRuleCore<T>(p1,statement, cacheKey)?.DynamicInvoke(parameter);
                }
                catch (Exception ex)
                {
                    throw;

                }
            }
        }
        private static Delegate ApplyRuleCore<T>(Type tType,string expression, string cacheKey) where T : class
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Delegate fun)
                {
                    return fun;
                }
            }
            var body = GofDynamicExpression.ParseLambda(new[] { BP(tType) }, null, expression, Symbols);

            var compiled = body.Compile();
            Cache.Add(cacheKey, compiled, DefaultPolicy);
            return compiled;
        }
        private static TValue AddOrGetExisting<TValue>(this MemoryCache cache, string key, Func<string, TValue> valueFactory, CacheItemPolicy policy, string regionName = null)
        {
            var lazy = new Lazy<TValue>(() => valueFactory(key));

            Lazy<TValue> item = (Lazy<TValue>)cache.AddOrGetExisting(key, lazy, policy, regionName) ?? lazy;

            return item.Value;
        }
        private static ParameterExpression BP(Type tp)
        {
            var key = Cache.AddOrGetExisting<string>($"className_{tp.Name}", (i) => tp.Name.GetShortName(), DefaultPolicy);
            var x = Expression.Parameter(tp, key);
            return x;
        }
        private static string GetShortName(this string longvalue)
        {
            //var sn=regex.Match(longvalue).Value;
            string sn = new string(longvalue.ToCharArray().Where(x => char.IsUpper(x)).Select(x => x).ToArray());
            if (string.IsNullOrWhiteSpace(sn)) sn = longvalue;
            return sn.ToLower();
        }
    }
    
}
