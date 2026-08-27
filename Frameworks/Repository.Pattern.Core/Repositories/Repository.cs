// ***********************************************************************
// Assembly         : Repository.Pattern.Core
// Author           : Mukesh Rebari
// Created          : 02-01-2016
//
// Last Modified By : Mukesh Rebari
// Last Modified On : 03-30-2016
// ***********************************************************************
// <copyright file="Repository.cs" company="India WEBLAB Pvt Ltd">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using LinqKit;
using Repository.Pattern.Core.Query;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Pattern.Core.Repositories
{
    /// <summary>
    /// Class Repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the t entity.</typeparam>
    /// <seealso cref="Repository.Pattern.Core.Repositories.IRepositoryAsync{TEntity}" />
    public class Repository<TEntity> : IRepositoryAsync<TEntity> where TEntity : class
    {
        public IUnitOfWorkAsync UOW { get; private set; }
        #region Private Fields

        //private readonly ITrackoApiDbContext _context;
        /// <summary>
        /// The _DB set
        /// </summary>
        private readonly DbSet<TEntity> _dbSet;
        /// <summary>
        /// The _unit of work
        /// </summary>
        private readonly IUnitOfWorkAsync _unitOfWork;

        #endregion Private Fields

        //public Repository(ITrackoApiDbContext context, IUnitOfWorkAsync unitOfWork)
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public Repository(IUnitOfWorkAsync unitOfWork)
        {
            _unitOfWork = unitOfWork;
            // Temporarily for FakeDbContext, Unit Test and Fakes
            var dbContext = unitOfWork.Context as DbContext;

            if (dbContext != null)
            {
                _dbSet = dbContext.Set<TEntity>();
                UOW = unitOfWork;
            }
        }

        /// <summary>
        /// Finds the specified key values.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        /// <returns>TEntity.</returns>
        public virtual TEntity Find(params object[] keyValues)
        {
            return _dbSet.Find(keyValues);
        }

        /// <summary>
        /// Selects the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>IQueryable&lt;TEntity&gt;.</returns>
        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return _dbSet.SqlQuery(query, parameters).AsQueryable();
        }

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>TEntity.</returns>
        public virtual TEntity Insert(TEntity entity)
        {
            return _dbSet.Add(entity);
        }

        /// <summary>
        /// Inserts the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void InsertRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Insert(entity);
            }
        }

        /// <summary>
        /// Inserts the graph range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void InsertGraphRange(IEnumerable<TEntity> entities)
        {
            _dbSet.AddRange(entities);
        }
        public virtual ObservableCollection<TEntity> Local => _dbSet.Local;
        public long GetDTSStatusIdByDateId(long dateId)
        {
            return UOW.Context.GetDTSStatusIdByDateId(dateId);
        }
        public async Task<string> ValidateTLDateRangeOverlap(DateTime tripStartDate, DateTime? tripEndDate, long ownvehicleid = 0, long hirevehicleid = 0, long triplogId = 0, long triptype = 1158, long tripnature = 0)
        {
            return await UOW.Context.ValidateTLDateRangeOverlap(tripStartDate, tripEndDate, ownvehicleid, hirevehicleid, triplogId, triptype, tripnature);
        }
        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Update(TEntity entity)
        {
            //var entry=_unitOfWork.Context.Entry(entity);
            _unitOfWork.Context.Attach(entity);
            //if (_unitOfWork.Context.Entry(entity).State == EntityState.Detached)
            //{
            //    var e = _dbSet.Attach(entity);
            //}
            //_dbSet.Attach(entity);
        }
        public virtual void Attach(TEntity entity)
        {            
            var state = _unitOfWork.Context.Entry(entity).State;
            if (state == EntityState.Detached)
            {

                _dbSet.Attach(entity);
            }
        }

        public void Detach(TEntity entity)
        {
            try
            {
                var entry = _unitOfWork.Context.Entry(entity);
                if (entry.State != EntityState.Detached)
                {
                    _unitOfWork.ObjectContext.Detach(entity);
                }
            }
            catch (Exception)
            {
               //ignore
            }
            
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="updatedProperties">The updated properties.</param>
        public virtual void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties)
        {
            var dbEntityEntry = _unitOfWork.Context.Entry(entity);
            if (updatedProperties.Any())
            {
                //update explicitly mentioned properties
                foreach (var property in updatedProperties)
                {
                    
                    dbEntityEntry.Property(property.Name).IsModified = true;
                }
            }
        }
        public virtual void UpdatePartial(TEntity entity, params Expression<Func<TEntity, object>>[] ignoredProperties)
        {
            var dbEntityEntry = _unitOfWork.Context.Entry(entity);
            if (ignoredProperties.Any())
            {
                //update explicitly mentioned properties
                foreach (var property in ignoredProperties)
                {

                    dbEntityEntry.Property(property.Name).IsModified = false;
                }
            }
        }

        public virtual int Delete(Expression<Func<TEntity, bool>> expression)
        {
           return _unitOfWork.Context.Delete(expression);
        }

        public virtual int Delete(IQueryable<TEntity> query)
        {
           return _unitOfWork.Context.Delete(query);
        }

        public virtual int Delete(ObjectQuery<TEntity> query)
        {
            return _unitOfWork.Context.Delete(query);
        }
        public string GetConfigValue(string key)
        {
            return UOW.Context.GetApiConfig(key);
        }
        
        public T GetConfigValue<T>(string key) where T : struct
        {
            return UOW.Context.GetApiConfig<T>(key);
        }
        public T GetClientConfigValue<T>(string key) where T : struct
        {
            return UOW.Context.GetApiClientConfig<T>(key);
        }
        public T GetConfigValue<T>(string key,T defaultValue) where T : struct
        {
            return UOW.Context.GetApiConfig<T>(key,defaultValue);
        }
        public T GetClientConfigValue<T>(string key, T defaultValue) where T : struct
        {
            return UOW.Context.GetApiClientConfig<T>(key, defaultValue);
        }
        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public virtual void Delete(object id)
        {
            var entity = _dbSet.Find(id);
            Delete(entity);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntity entity)
        {
            _dbSet.Attach(entity);
        }

        /// <summary>
        /// Queries the specified query object.
        /// </summary>
        /// <returns>IQueryFluent&lt;TEntity&gt;.</returns>
        public IQueryFluent<TEntity> Query()
        {
            return new QueryFluent<TEntity>(this);
        }

        /// <summary>
        /// Queries the specified query object.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>IQueryFluent&lt;TEntity&gt;.</returns>
        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject)
        {
            return new QueryFluent<TEntity>(this, queryObject);
        }

        /// <summary>
        /// Queries the specified query object.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IQueryFluent&lt;TEntity&gt;.</returns>
        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            return new QueryFluent<TEntity>(this, query);
        }

        /// <summary>
        /// Queryables this instance.
        /// </summary>
        /// <returns>IQueryable&lt;TEntity&gt;.</returns>
        public IQueryable<TEntity> Queryable()
        {
            return _dbSet;
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IRepository&lt;T&gt;.</returns>
        public IRepository<T> GetRepository<T>() where T : class
        {
            return _unitOfWork.Repository<T>();
        }

        public IQueryable<TViewEntity> SelectQuery<TViewEntity>(string query, params object[] parameters) where TViewEntity : class
        {
           return _unitOfWork.Context.Database.SqlQuery<TViewEntity>(query, parameters).AsQueryable();
        }


        /// <summary>
        /// find as an asynchronous operation.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;TEntity&gt;.</returns>
        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await _dbSet.FindAsync(keyValues);
        }

        /// <summary>
        /// find as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;TEntity&gt;.</returns>
        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return await _dbSet.FindAsync(cancellationToken, keyValues);
        }

        /// <summary>
        /// delete as an asynchronous operation.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public virtual async Task<bool> DeleteAsync(params object[] keyValues)
        {
            return await DeleteAsync(CancellationToken.None, keyValues);
        }

        /// <summary>
        /// delete as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            var entity = await FindAsync(cancellationToken, keyValues);

            if (entity == null)
            {
                return false;
            }
            _dbSet.Attach(entity);

            return true;
        }


        /// <summary>
        /// Selects the specified filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="includes">The includes.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>IQueryable&lt;TEntity&gt;.</returns>
        internal IQueryable<TEntity> Select(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            if (filter != null)
            {
                query = query.AsExpandable().Where(filter);
            }
            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }
            return query;
        }

        /// <summary>
        /// select as an asynchronous operation.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="includes">The includes.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Task&lt;IEnumerable&lt;TEntity&gt;&gt;.</returns>
        internal async Task<IEnumerable<TEntity>> SelectAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            List<Expression<Func<TEntity, object>>> includes = null,
            int? page = null,
            int? pageSize = null)
        {
            return await Select(filter, orderBy, includes, page, pageSize).ToListAsync();
        }

        /// <summary>
        /// Inserts the or update graph.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void InsertOrUpdateGraph(TEntity entity)
        {
            EntitesChecked = null;
            _dbSet.Attach(entity);
        }

        /// <summary>
        /// The entites checked
        /// </summary>
        public HashSet<object> EntitesChecked; // tracking of all process entities in the object graph when calling SyncObjectGraph

        public virtual int ExecuteSql(string query)
        {
            return this._unitOfWork.Context.Database.ExecuteSqlCommand(query);
        }
        public virtual async Task<int> ExecuteSqlAsync(string query)
        {
            return await this._unitOfWork.Context.Database.ExecuteSqlCommandAsync(query);
        }
        public virtual async Task<int> ExecuteSqlAsync(string query,params object[] parameters)
        {
            return await this._unitOfWork.Context.Database.ExecuteSqlCommandAsync(query, parameters);
        }
    }
}
