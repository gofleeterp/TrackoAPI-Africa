using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Query;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

namespace Service.Pattern
{
    public interface IService<TEntity> where TEntity:class
    {
        TEntity Find(params object[] keyValues);
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);
        TEntity Insert(TEntity entity);
        void InsertRange(IEnumerable<TEntity> entities);
        void InsertOrUpdateGraph(TEntity entity);
        void InsertGraphRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties);
        void Delete(object id);
        void Delete(TEntity entity);
        IQueryFluent<TEntity> Query();
        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        Task<TEntity> FindAsync(params object[] keyValues);
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        Task<bool> DeleteAsync(params object[] keyValues);
        Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues);
        IQueryable<TEntity> Queryable();
        int ExecuteSql(string sqlQuery);
        Task<int> ExecuteSqlAsync(string sqlQuery);

        HttpRequestMessage Request { get; set; }
        int Delete(Expression<Func<TEntity, bool>> expression);
        int Delete(IQueryable<TEntity> query);
        int Delete(ObjectQuery<TEntity> query);
        string GetConfigValue(string key);
        T GetConfigValue<T>(string key) where T:struct;
        void Patch(TEntity entity);
        ObservableCollection<TEntity> Local { get; }
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression);
        Task<int> DeleteAsync(IQueryable<TEntity> query);
        Task<int> DeleteAsync(ObjectQuery<TEntity> query);
        Task InsertRangeAsync(IEnumerable<TEntity> entities);
        Task InsertOrUpdateGraphAsync(TEntity entity);
        Task InsertGraphRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateAsync(TEntity entity);
        Task UpdateAsync(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties);
        Task DeleteAsync(object id);
        Task DeleteAsync(TEntity entity);
        Task<TEntity> InsertAsync(TEntity entity);
        Task PatchAsync(TEntity entity);
        Task<string> ValidateTLDateRangeOverlapAsync(DateTime tripStartDate, DateTime? tripEndDate, long ownvehicleid = 0, long hirevehicleid = 0, long triplogId = 0, long triptype = 1158, long tripnature = 0);
    }
}