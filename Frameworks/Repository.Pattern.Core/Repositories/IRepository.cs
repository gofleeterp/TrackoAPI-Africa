// ***********************************************************************
// Assembly         : Repository.Pattern.Core
// Author           : Mukesh Rebari
// Created          : 02-01-2016
//
// Last Modified By : Mukesh Rebari
// Last Modified On : 03-30-2016
// ***********************************************************************
// <copyright file="IRepository.cs" company="India WEBLAB Pvt Ltd">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************
using Repository.Pattern.Core.Query;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repository.Pattern.Core.Repositories
{
    /// <summary>
    /// Interface IRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the t entity.</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        IUnitOfWorkAsync UOW { get; }

        /// <summary>
            /// Finds the specified key values.
            /// </summary>
            /// <param name="keyValues">The key values.</param>
            /// <returns>TEntity.</returns>
        TEntity Find(params object[] keyValues);
        /// <summary>
        /// Selects the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>IQueryable&lt;TEntity&gt;.</returns>
        IQueryable<TEntity> SelectQuery(string query, params object[] parameters);
        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>TEntity.</returns>
        TEntity Insert(TEntity entity);
        /// <summary>
        /// Inserts the range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void InsertRange(IEnumerable<TEntity> entities);
        /// <summary>
        /// Inserts the or update graph.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void InsertOrUpdateGraph(TEntity entity);
        /// <summary>
        /// Inserts the graph range.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void InsertGraphRange(IEnumerable<TEntity> entities);
        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="updatedProperties">The updated properties.</param>
        void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties);
        void UpdatePartial(TEntity entity, params Expression<Func<TEntity, object>>[] ignoredProperties);
        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void Delete(object id);
        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete(TEntity entity);
        /// <summary>
        /// Queries the specified query object.
        /// </summary>
        /// <param name="queryObject">The query object.</param>
        /// <returns>IQueryFluent&lt;TEntity&gt;.</returns>
        IQueryFluent<TEntity> Query(IQueryObject<TEntity> queryObject);
        /// <summary>
        /// Queries the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IQueryFluent&lt;TEntity&gt;.</returns>
        IQueryFluent<TEntity> Query(Expression<Func<TEntity, bool>> query);
        /// <summary>
        /// Queries this instance.
        /// </summary>
        /// <returns>IQueryFluent&lt;TEntity&gt;.</returns>
        IQueryFluent<TEntity> Query();
        /// <summary>
        /// Queryables this instance.
        /// </summary>
        /// <returns>IQueryable&lt;TEntity&gt;.</returns>
        IQueryable<TEntity> Queryable();
        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IRepository&lt;T&gt;.</returns>
        IRepository<T> GetRepository<T>() where T : class;
        IQueryable<TViewEntity> SelectQuery<TViewEntity>(string query, params object[] parameters) where TViewEntity : class;

        int ExecuteSql(string sqlQuery);
        void Attach(TEntity entity);
        void Detach(TEntity entity);
        int Delete(Expression<Func<TEntity, bool>> expression);
        int Delete(IQueryable<TEntity> query);
        int Delete(ObjectQuery<TEntity> query);
        string GetConfigValue(string key);
        T GetConfigValue<T>(string key) where T : struct;
        T GetClientConfigValue<T>(string key) where T : struct;
        T GetConfigValue<T>(string key,T defaultValue) where T : struct;
        T GetClientConfigValue<T>(string key, T defaultValue) where T : struct;
        ObservableCollection<TEntity> Local { get; }
        long GetDTSStatusIdByDateId(long dateId);
        Task<string> ValidateTLDateRangeOverlap(DateTime tripStartDate, DateTime? tripEndDate, long ownvehicleid = 0, long hirevehicleid = 0, long triplogId = 0, long triptype = 1158, long tripnature = 0);
    }
}
