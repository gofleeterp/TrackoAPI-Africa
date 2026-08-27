// ***********************************************************************
// Assembly         : Repository.Pattern.Core
// Author           : Admin
// Created          : 02-01-2016
//
// Last Modified By : Admin
// Last Modified On : 03-29-2016
// ***********************************************************************
// <copyright file="UnitOfWork.cs" company="">
//     Copyright ©  2016
// </copyright>
// <summary></summary>
// ***********************************************************************
#region

using EntityFramework.BulkInsert.Extensions;
using Newtonsoft.Json;

using Repository.Pattern.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoAPI.SignalR.Core;

#endregion

namespace Repository.Pattern.Core.UnitOfWork
{
    /// <summary>
    /// Class UnitOfWork.
    /// </summary>
    /// <seealso cref="Repository.Pattern.Core.UnitOfWork.IUnitOfWorkAsync" />
    public class UnitOfWork : IUnitOfWorkAsync
    {
        #region Private Fields

        public readonly IClientHub SGL;

        /// <summary>
        /// The _data context
        /// </summary>
        private ITrackoApiDbContext _dataContext;

        /// <summary>
        /// The _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// The _object context
        /// </summary>
        private ObjectContext _objectContext;

        /// <summary>
        /// The _repositories
        /// </summary>
        private Dictionary<string, dynamic> _repositories;

        /// <summary>
        /// The _transaction
        /// </summary>
        private DbTransaction _transaction;
        #endregion Private Fields

        #region Constuctor/Dispose

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public UnitOfWork(ITrackoApiDbContext dataContext, IClientHub signalr)
        {
            _dataContext = dataContext;
            //_objectContext=_dataContext as ObjectContext;
            _objectContext = ((IObjectContextAdapter)_dataContext).ObjectContext;

            //var db = _dataContext as DbContext;

            _repositories = new Dictionary<string, dynamic>();
            SGL = signalr;
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public ITrackoApiDbContext Context => _dataContext;

        public bool IsODataBatchContext { get; set; } = false;
        public ObjectContext ObjectContext { get { return _objectContext; } }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level.</param>
        /// <exception cref="ObjectDisposedException">When the <see cref="T:System.Data.Entity.Core.Objects.ObjectContext" /> instance has been disposed.</exception>
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (IsODataBatchContext) return;
            IsODataBatchContext = false;
            //_objectContext = ((IObjectContextAdapter) _dataContext).ObjectContext;
            if (_objectContext.Connection.State != ConnectionState.Open)
            {
                _objectContext.Connection.Open();
            }
            //if (_dataContext.Database.CurrentTransaction == null)
            //{
            //    _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
            //}
            _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
        }

        public void BulkInsert<T>(List<T> entities, SqlRowsCopiedEventHandler callback=null)
        {
            var options = new BulkInsertOptions
            {
                EnableStreaming = true,
                BatchSize = 5000,
                TimeOut = 180,
                SqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints,
                Callback=callback
            };
            this.BulkInsert(entities, options);
        }

        public void BulkInsert<T>(List<T> entities, IDbTransaction transaction)
        {
            var options = new BulkInsertOptions
            {
                EnableStreaming = true,
                BatchSize = 5000,
                TimeOut = 180,
                SqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints
            };
            this.BulkInsert(entities, transaction, options);
        }

        public void BulkInsert<T>(List<T> entities, BulkInsertOptions options)
        {
            try
            {
                ((DbContext)_dataContext).BulkInsert(entities, options);
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void BulkInsert<T>(List<T> entities, IDbTransaction transaction, BulkInsertOptions options)
        {
            try
            {
                ((DbContext)_dataContext).BulkInsert(entities, transaction, options);
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Commits this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Commit()
        {
            if (IsODataBatchContext||_transaction==null) return true;
            
            using (_transaction)
            {
                _transaction.Commit();
            }
            //_transaction.Commit();
            //_transaction.Dispose();
            return true;
        }

        public int DeleteStockByChallanId(long challanId)
        {
            return _dataContext.Database.ExecuteSqlCommand($"EXEC Proc_DeleteOutStockLog @ChallanCnId={challanId}");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <exception cref="DbException">The connection-level error that occurred while opening the connection. </exception>
        public virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // free other managed objects that implement
                // IDisposable only

                try
                {
                    if (_objectContext != null && _objectContext.Connection.State == ConnectionState.Open)
                    {
                        _objectContext.Connection.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // do nothing, the objectContext has already been disposed
                }

                if (_dataContext != null)
                {
                    _dataContext.Dispose();
                    _dataContext = null;
                }
            }

            // release any unmanaged objects
            // set the object references to null

            _disposed = true;
        }

        public async Task<int> ExecSqlQueryAsync(string sql, params object[] parameters)
        {
            return await _dataContext.Database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, sql, parameters);
        }

        public async Task<int> ExecuteProcedureAsync(string sql, params object[] parameters)
        {
            var existingconnection = _dataContext.Database.CurrentTransaction != null || _dataContext.Database.Connection.State == ConnectionState.Open;
            var connection = _dataContext.Database.CurrentTransaction?.UnderlyingTransaction?.Connection ?? _dataContext.Database.Connection;

            using (System.Data.IDbCommand command = connection.CreateCommand())
            {
                int count = 0;
                try
                {
                    if (!existingconnection)
                    {
                        await connection.OpenAsync();
                    }
                    else
                    {
                        command.Transaction = _dataContext.Database.CurrentTransaction?.UnderlyingTransaction;
                    }
                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    count = command.ExecuteNonQuery();
                }
                finally
                {
                    if (!existingconnection)
                        connection.Close();
                    command.Parameters.Clear();
                }
                return count;
            }
        }

        public DbTransaction ODataBatchBeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            //_objectContext = ((IObjectContextAdapter) _dataContext).ObjectContext;
            if (_objectContext.Connection.State != ConnectionState.Open)
            {
                _objectContext.Connection.Open();
            }
            //if (_dataContext.Database.CurrentTransaction == null)
            //{
            //    _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
            //}
            IsODataBatchContext = true;
            return _objectContext.Connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Repositories this instance.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <returns>IRepository&lt;TEntity&gt;.</returns>
        public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            return RepositoryAsync<TEntity>();
        }

        /// <summary>
        /// Repositories the asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the t entity.</typeparam>
        /// <returns>IRepositoryAsync&lt;TEntity&gt;.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2" />.</exception>
        /// <exception cref="InvalidOperationException">The current type does not represent a generic type definition. That is, <see cref="P:System.Type.IsGenericTypeDefinition" /> returns false. </exception>
        /// <exception cref="NotSupportedException">The invoked method is not supported in the base class. Derived classes must provide an implementation.</exception>
        public IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class
        {
            //if (ServiceLocator.IsLocationProviderSet)
            //{
            //    return ServiceLocator.Current.GetInstance<IRepositoryAsync<TEntity>>();
            //}

            if (_repositories == null)
            {
                _repositories = new Dictionary<string, dynamic>();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
            {
                return (IRepositoryAsync<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(Repository<>);

            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), this));

            return _repositories[type];
        }

        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        public void Rollback()
        {
            if (IsODataBatchContext|| _transaction == null) return;
            using (_transaction)
            {
                _transaction.Rollback();
            }
            //_transaction.Rollback();
            //_transaction.Dispose();
            //_dataContext.SyncObjectsStatePostCommit();
        }

        /// <exception cref="OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public int SaveChanges(SaveOptions options)
        {
            return _objectContext.SaveChanges(options);
        }
        #endregion Constuctor/Dispose
        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int SaveChanges()
        {
            return _dataContext.SaveChanges();
        }
        /// <summary>
        /// Saves the changes asynchronous.
        /// </summary>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public Task<int> SaveChangesAsync()
        {
            return _dataContext.SaveChangesAsync();
        }

        /// <exception cref="OptimisticConcurrencyException">An optimistic concurrency violation has occurred while saving changes.</exception>
        public Task<int> SaveChangesAsync(SaveOptions options)
        {
            return _objectContext.SaveChangesAsync(options);
        }

        /// <summary>
        /// Saves the changes asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dataContext.SaveChangesAsync(cancellationToken);
        }
        #region Unit of Work Transactions
        #endregion
        public async Task<string> SqlQueryAsJsonAsync(string sql, params object[] parameters)
        {
            var database = _dataContext.Database;
            var jsonResult = new StringBuilder();
            //var result=await _objectContext.ExecuteStoreQueryAsync<string>(sql, parameters);
            using (System.Data.IDbCommand command = database.Connection.CreateCommand())
            {
                try
                {
                    await database.Connection.OpenAsync();
                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    using (System.Data.IDataReader rd = command.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            jsonResult.Append(rd.GetValue(0));
                        }
                    }
                }
                finally
                {
                    database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            var result = jsonResult.ToString();
            return string.IsNullOrWhiteSpace(result) ? "[]" : result;
        }
        public async Task<DataTable> SqlQueryAsync(string sql, params object[] parameters)
        {
            var existingconnection = _dataContext.Database.CurrentTransaction != null || _dataContext.Database.Connection.State == ConnectionState.Open;
            var connection = _dataContext.Database.CurrentTransaction?.UnderlyingTransaction?.Connection ?? _dataContext.Database.Connection;
            var dt = new DataTable();
            using (System.Data.IDbCommand command = connection.CreateCommand())
            {
                try
                {
                    if (!existingconnection)
                    {
                        await connection.OpenAsync();
                    }
                    else
                    {
                        command.Transaction = _dataContext.Database.CurrentTransaction?.UnderlyingTransaction;
                    }

                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
                finally
                {
                    if (!existingconnection)
                        connection.Close();
                    command.Parameters.Clear();
                }
            }
            return dt;
        }

        public async Task<DataSet> SqlQueryDataSetAsync(string sql, IDictionary<string, string> tableNameMapping = null, params object[] parameters)
        {
            var database = _dataContext.Database;
            var dt = new DataSet();
            using (DbCommand command = database.Connection.CreateCommand())
            {
                try
                {
                    await database.Connection.OpenAsync();
                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    var factory = DbProviderFactories.GetFactory(database.Connection);
                    var adapter = factory.CreateDataAdapter();
                    if (adapter == null) throw new BusinessException(ErrorCode.GLB107, "Invalide Database Type");
                    if (tableNameMapping != null && tableNameMapping.Count > 0)
                    {
                        var mapping = tableNameMapping.Select(x => new DataTableMapping(x.Key, x.Value)).ToArray();
                        adapter.TableMappings.AddRange(mapping);
                    }
                    adapter.SelectCommand = command;
                    adapter.Fill(dt, "Table");
                }
                finally
                {
                    database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return dt;
        }
    }
}