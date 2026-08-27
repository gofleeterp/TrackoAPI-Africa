using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TrackoApi.Models.Validations;

namespace Tenant.Models
{
    public interface ITenantEntitySet<TEntity> where TEntity : class
    {
        ITenantDbContext Context { get; }
        DbSet<TEntity> Table { get; }
        Database Database { get; }
        TResult GetBySqlQuery<TResult>(string query, params object[] parameters);
        TEntity GetBySqlQuery(string query, params object[] parameters);
        TEntity Add(TEntity entity);
        TEntity Attach(TEntity entity);
        TEntity Create();
        TEntity Find(params object[] keyValues);
        TEntity Delete(TEntity entity);
        IEnumerable<TEntity> Local { get; }
        IQueryable<TEntity> Query { get; }
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        Task<TEntity> FindAsync(params object[] keyValues);
    }
    public class TenantEntitySet<TEntity> : ITenantEntitySet<TEntity> where TEntity : class
    {
        private readonly ITenantDbContext _ctx;
        //[InjectionConstructor]
        public TenantEntitySet(ITenantDbContext ctx)
        {
            _ctx = ctx;
        }
        public ITenantDbContext Context => _ctx;
        private DbSet<TEntity> _enititySet;
        public DbSet<TEntity> Table =>_enititySet?? _ctx.Set<TEntity>();
        public Database Database => _ctx.Database;
        public TResult GetBySqlQuery<TResult>(string query, params object[] parameters)
        {
            return _ctx.Database.SqlQuery<TResult>(query, parameters).FirstOrDefault();
        }
        public TEntity GetBySqlQuery(string query, params object[] parameters)
        {
            return Table.SqlQuery(query, parameters).FirstOrDefault();
        }
        public IQueryable<TEntity> Query() => Table.AsQueryable();
        #region IDbSet Methods

        public TEntity Add(
            TEntity entity)
        {
            return (TEntity)Table.Add(entity);
        }

        public TEntity Attach(
            TEntity entity)
        {
            return (TEntity)Table.Attach(entity);
        }

        public TEntity Create()
        {
            return (TEntity)Table.Create();
        }

        public TEntity Find(
            params object[] keyValues)
        {
            return (TEntity)Table.Find(keyValues);
        }
        public TEntity Delete(
            TEntity entity)
        {
            return (TEntity)Table.Remove(entity);
        }

        public Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return this.Table.FindAsync(cancellationToken, keyValues);
        }

        public Task<TEntity> FindAsync(params object[] keyValues)
        {
            return this.Table.FindAsync(keyValues);
        }

        public IEnumerable<TEntity> Local
        {
            get { return Table.Local; }
        }

        IQueryable<TEntity> ITenantEntitySet<TEntity>.Query { get; }
        #endregion

    }
}
