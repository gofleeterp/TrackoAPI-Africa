// ***********************************************************************
// Assembly         : Repository.Pattern.Core
// Author           : Mukesh Rebari
// Created          : 02-01-2016
//
// Last Modified By : Mukesh Rebari
// Last Modified On : 03-30-2016
// ***********************************************************************
// <copyright file="IRepositoryAsync.cs" company="India WEBLAB Pvt Ltd">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Repository.Pattern.Core.Repositories
{
    /// <summary>
    /// Interface IRepositoryAsync
    /// </summary>
    /// <typeparam name="TEntity">The type of the t entity.</typeparam>
    /// <seealso cref="Repository.Pattern.Core.Repositories.IRepository{TEntity}" />
    public interface IRepositoryAsync<TEntity> : IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Finds the asynchronous.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;TEntity&gt;.</returns>
        Task<TEntity> FindAsync(params object[] keyValues);
        /// <summary>
        /// Finds the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;TEntity&gt;.</returns>
        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);
        /// <summary>
        /// Deletes the asynchronous.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> DeleteAsync(params object[] keyValues);
        /// <summary>
        /// Deletes the asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="keyValues">The key values.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> DeleteAsync(CancellationToken cancellationToken, params object[] keyValues);

        
        Task<int> ExecuteSqlAsync(string sqlQuery);
        Task<int> ExecuteSqlAsync(string query, params object[] parameters);
    }
}