using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

namespace Repository.Pattern.Ef6.Extentions
{
    public static class DbRuleExtenation
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
        public static bool VaidateDbEntry<TEntity, TContext>(this DbEntityEntry parameter, TEntity entity, TContext ctx, string predicate, long ruleid = 0, string dbname = "")
            where TContext : class
            where TEntity : Entity
        {
            try
            {
                var p1 = entity.GetType();
                var cacheKey = $"{(!string.IsNullOrWhiteSpace(dbname)?dbname: Helper.LoggedInTenantId)}{(ruleid>0?$"{ruleid}": $"{p1.Name}_{predicate}")}";               
                var result = ValidateCore<TEntity, TContext>(parameter.GetType(), p1, ctx.GetType(), predicate, cacheKey).DynamicInvoke(ctx, entity, parameter);
                return (bool)result;
            }
            catch (Exception e)
            {
                throw;
            }

        }
        private static Delegate ValidateCore<TEntity, TContext>(Type dbEntry,Type tEntity,Type tContext, string predicate, string cacheKey)
            where TEntity : Entity
        {
            if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(nameof(cacheKey));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Func<TContext, TEntity, DbEntityEntry, bool> fun) return fun;
            }
            var expressions = TrackoApi.Core.GofDynamicExpression.ParseLambda(new ParameterExpression[] { BP(tContext), BP(tEntity), BP(dbEntry) }, typeof(bool),
                predicate, Symbols);
            var compiled = expressions.Compile();
            Cache.Add(cacheKey, compiled, DefaultPolicy);
            return compiled;
        }


        public static bool VaidateDb<TEntity, TContext>(this TEntity parameter, TContext ctx, string predicate, long ruleid = 0, string dbname = "") where TEntity : Entity
        {
            try
            {
                var p1 = parameter.GetType();
                var cacheKey = $"{(!string.IsNullOrWhiteSpace(dbname) ? dbname : Helper.LoggedInTenantId)}{(ruleid > 0 ? $"{ruleid}" : $"{p1.Name}_{predicate}")}";
                return ValidateCore<TEntity, TContext>(p1, ctx.GetType(), predicate, cacheKey)(parameter, ctx);
            }
            catch (Exception e)
            {
                throw;
            }

        }
        public static bool VaidateDb<TEntity>(this TEntity parameter, string predicate, long ruleid = 0, string dbname = "") where TEntity : Entity
        {
            try
            {
                var type = parameter.GetType();
                var cacheKey = $"{(!string.IsNullOrWhiteSpace(dbname) ? dbname : Helper.LoggedInTenantId)}{(ruleid > 0 ? $"{ruleid}" : $"{type.Name}_{predicate}")}";
                return ValidateCore<TEntity>(type, predicate, cacheKey)(parameter);
            }
            catch (Exception e)
            {
                throw;
            }

        }
        private static Func<TEntity, TContext, bool> ValidateCore<TEntity, TContext>(Type tEntity, Type tContext, string predicate, string cacheKey) where TEntity : Entity
        {
            if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(nameof(cacheKey));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Func<TEntity, TContext, bool> fun) return fun;
            }
            Expression<Func<TEntity, TContext, bool>> lambda = (Expression<Func<TEntity, TContext, bool>>)TrackoApi.Core.GofDynamicExpression.ParseLambda(new ParameterExpression[] { BP(tEntity), BP(tContext) }, typeof(bool),
                predicate, Symbols);
            var compiled = lambda.Compile();
            Cache.Add(cacheKey, compiled, DefaultPolicy);
            return compiled;
        }
        
        public static void ApplyDbRule<TEntity, TContext>(this DbEntityEntry dbentry, TEntity entity, /*Type ETType,*/ TContext ctx, string expression, long ruleid = 0, string dbname = "")
            where TEntity : Entity
            where TContext : class
        {
            //var internal_entity = Convert.ChangeType(entity, ETType);
            var statements = expression.Split(';');
            var tp2 = entity.GetType();
            var tp1 = dbentry.GetType();
            var ctxType = ctx.GetType();
            foreach (var statement in statements)
            {
                if (string.IsNullOrWhiteSpace(statement)) continue;
                try
                {
                    
                    var cacheKey = $"{(!string.IsNullOrWhiteSpace(dbname) ? dbname : Helper.LoggedInTenantId)}{(ruleid > 0 ? $"{ruleid}" : $"{tp1.Name}_{tp2.Name}")}_{statement}";
                    ApplyRuleCore(tp1, tp2, ctxType, statement, cacheKey).DynamicInvoke(dbentry, entity, ctx);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public static void ApplyDbRule<TEntity1, TEntity2, TContext>(this TEntity1 parameter1, TEntity2 parameter2, TContext context, string expression, long ruleid = 0, string dbname = "")
           where TEntity1 : Entity
           where TEntity2 : Entity
        {
            var tp2 = parameter2.GetType();
            var tp1 = parameter1.GetType();
            var ctxType = context.GetType();
            var statements = expression.Split(';');
            foreach (var statement in statements)
            {
                try
                {
                   
                    var cacheKey = $"{(!string.IsNullOrWhiteSpace(dbname) ? dbname : Helper.LoggedInTenantId)}{(ruleid > 0 ? $"{ruleid}" : $"{tp1.Name}_{tp2.Name}")}_{statement}";
                    ApplyRuleCore(tp1, tp2, ctxType, statement, cacheKey)?.DynamicInvoke(parameter1, parameter2, context);
                }
                catch (Exception ex)
                {
                    throw;

                }
            }
        }


        public static void ApplyDbRule<TEntity, TContext>(this TEntity parameter,TContext context, string expression, long ruleid = 0, string dbname = "") where TEntity : Entity
        {
            var p1 = parameter.GetType();
            var ctx = context.GetType();
            var statements = expression.Split(';');
            foreach (var statement in statements)
            {
                try
                {
                    var cacheKey = $"{(!string.IsNullOrWhiteSpace(dbname) ? dbname : Helper.LoggedInTenantId)}{(ruleid > 0 ? $"{ruleid}" : $"{p1.Name}")}_{statement}";
                    ApplyRuleCore(p1, ctx, statement, cacheKey)?.DynamicInvoke(parameter, context);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        private static Func<TEntity, bool> ValidateCore<TEntity>(Type type,string predicate, string cacheKey) where TEntity : Entity
        {
            if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(nameof(cacheKey));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Func<TEntity, bool> fun) return fun;
            }
            //ParameterExpression x = Expression.Parameter(typeof(TEntity), "entity");
            Expression<Func<TEntity, bool>> lambda = (Expression<Func<TEntity, bool>>)TrackoApi.Core.GofDynamicExpression.ParseLambda(new ParameterExpression[] { BP(type) }, typeof(bool),
                predicate, Symbols);
            var compiled = lambda.Compile();
            Cache.Add(cacheKey, compiled, DefaultPolicy);
            return compiled;
        }
        private static Delegate ApplyRuleCore(Type TEntity1, Type TEntity2, Type TContext, string expression, string cacheKey)
        {
            //var cacheKey = ruleid > 0 ? $"{ClientInfo.Instance.AccessToken?.TenantId}_{ruleid}" : $"{typeof(TEntity).Name}_{expression}_{ruleid}";
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Delegate fun)
                {
                    return fun;
                }
            }
            var body = GofDynamicExpression.ParseLambda(new[] { BP(TEntity1), BP(TEntity2), BP(TContext) }, null, expression, Symbols);
            var compiled = body.Compile();
            Cache.Add(cacheKey, compiled, DefaultPolicy);
            return compiled;
        }
        private static Delegate ApplyRuleCore(Type TEntity,Type TContext,string expression, string cacheKey)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (Cache.Contains(cacheKey))
            {
                if (Cache.Get(cacheKey) is Delegate fun)
                {
                    return fun;
                }
            }
            var body = GofDynamicExpression.ParseLambda(new[] { BP(TEntity), BP(TContext) }, null, expression, Symbols);

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
            var key = Cache.AddOrGetExisting<string>($"className_{tp.Name}", (i)=> tp.Name.GetShortName(), DefaultPolicy);
            var x= Expression.Parameter(tp, key);
            return x;
        }
        private static string GetShortName(this string longvalue)
        {
            //var sn=regex.Match(longvalue).Value;
            string sn = new string(longvalue.ToCharArray().Where(x=>char.IsUpper(x)).Select(x=>x).ToArray());
            if (string.IsNullOrWhiteSpace(sn)) sn = longvalue;
            return sn.ToLower();
        }
    }
}
