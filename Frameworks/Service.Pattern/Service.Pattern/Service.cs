using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Query;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Queue;

namespace Service.Pattern
{
    public abstract class Service<TEntity> : IService<TEntity> where TEntity : class
    {
        #region Private Fields
        private readonly IRepositoryAsync<TEntity> _repository;
        #endregion Private Fields

        #region Constructor

        protected Service(IRepositoryAsync<TEntity> repository)
        {
            _repository = repository;
        }
        #endregion Constructor

        public virtual TEntity Find(params object[] keyValues)
        {
            return _repository.Find(keyValues);
        }

        public virtual IQueryable<TEntity> SelectQuery(string query, params object[] parameters)
        {
            return _repository.SelectQuery(query, parameters).AsQueryable();
        }

        public virtual TEntity Insert(TEntity entity)
        {
           return _repository.Insert(entity);
        }

        public virtual void InsertRange(IEnumerable<TEntity> entities)
        {
            _repository.InsertRange(entities);
        }

        public virtual void InsertOrUpdateGraph(TEntity entity)
        {
            _repository.InsertOrUpdateGraph(entity);
        }

        public virtual void InsertGraphRange(IEnumerable<TEntity> entities)
        {
            _repository.InsertGraphRange(entities);
        }

        public virtual void Update(TEntity entity)
        {
            _repository.Update(entity);
        }

        public virtual void Patch(TEntity entity)
        {
            
        }
        
        public ObservableCollection<TEntity> Local
        {
            get { return _repository.Local; }
        }

        public virtual void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties)
        {
            _repository.Update(entity,updatedProperties);
        }
        public virtual void Delete(object id)
        {
            _repository.Delete(id);
        }

        public virtual void Delete(TEntity entity)
        {
            _repository.Delete(entity);
        }

        public IQueryFluent<TEntity> Query()
        {
            return _repository.Query();
        }

        public virtual IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject)
        {
            return _repository.Query(queryObject);
        }

        public virtual IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query)
        {
            return _repository.Query(query);
        }

        public virtual async Task<TEntity> FindAsync(params object[] keyValues)
        {
            return await _repository.FindAsync(keyValues);
        }

        public virtual async Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return await _repository.FindAsync(cancellationToken, keyValues);
        }

        public virtual async Task<bool> DeleteAsync(params object[] keyValues)
        {
            return await DeleteAsync(CancellationToken.None, keyValues);
        }

        //IF 04/08/2014 - Before: return await DeleteAsync(cancellationToken, keyValues);
        public virtual async Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return await _repository.DeleteAsync(cancellationToken, keyValues);
        }

        public IQueryable<TEntity> Queryable()
        {
            return _repository.Queryable();
        }

        public virtual int ExecuteSql(string sqlQuery)
        {
            try
            {
                return _repository.ExecuteSql(sqlQuery);
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ex);
            }
            
        }

        public virtual async Task<int> ExecuteSqlAsync(string sqlQuery)
        {
            try
            {
                return await _repository.ExecuteSqlAsync(sqlQuery);
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ex);
            }
        }

        public HttpRequestMessage Request { get; set; }
        public virtual int Delete(Expression<Func<TEntity, bool>> expression)
        {
            return _repository.Delete(expression);
        }

        public virtual int Delete(IQueryable<TEntity> query)
        {
            return _repository.Delete(query);
        }

        public virtual int Delete(ObjectQuery<TEntity> query)
        {
            return _repository.Delete(query);
        }
        public string GetConfigValue(string key)
        {
            return _repository.GetConfigValue(key);
        }
        public T GetConfigValue<T>(string key) where T : struct
        {
            return _repository.GetConfigValue<T>(key);
        }
        public T GetConfigValue<T>(string key, T defaultValue) where T : struct
        {
            return _repository.GetConfigValue<T>(key,defaultValue);
        }
        public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {

            return await Task.Run(() => this.Delete(expression));
        }

        public virtual async Task<int> DeleteAsync(IQueryable<TEntity> query)
        {
            return await Task.Run(() => this.Delete(query));
        }

        public virtual async Task<int> DeleteAsync(ObjectQuery<TEntity> query)
        {
            return await Task.Run(() => this.Delete(query));
        }

        public virtual async Task InsertRangeAsync(IEnumerable<TEntity> entities)
        {
            await Task.Run(() => this.InsertRange(entities));
        }

        public virtual async Task InsertOrUpdateGraphAsync(TEntity entity)
        {
            await Task.Run(() => this.InsertOrUpdateGraph(entity));
        }

        public virtual async Task InsertGraphRangeAsync(IEnumerable<TEntity> entities)
        {
            await Task.Run(() => this.InsertGraphRange(entities));
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            await Task.Run(() => this.Update(entity));
        }

        public virtual async Task UpdateAsync(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties)
        {
            await Task.Run(() => this.Update(entity, updatedProperties));
        }

        public virtual async Task DeleteAsync(object id)
        {
            await Task.Run(() => Delete(id));
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            await Task.Run(() => Delete(entity));
        }
        public virtual async Task PatchAsync(TEntity entity)
        {
            await Task.Run(() => Patch(entity));
        }
        public virtual Task<TEntity> InsertAsync(TEntity entity)
        {
            return Task.Run(()=> _repository.Insert(entity));
        }

        public async Task<string> ValidateTLDateRangeOverlapAsync(DateTime tripStartDate, DateTime? tripEndDate, long ownvehicleid = 0, long hirevehicleid = 0, long triplogId = 0, long triptype = 1158, long tripnature = 0)
        {
            return await _repository.ValidateTLDateRangeOverlap(tripStartDate, tripEndDate, ownvehicleid, hirevehicleid, triplogId, triptype, tripnature);
        }
    }
}