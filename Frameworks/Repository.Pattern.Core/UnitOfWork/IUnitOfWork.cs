using EntityFramework.BulkInsert.Extensions;
using Repository.Pattern.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using TrackoApi.Data;

namespace Repository.Pattern.Core.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ITrackoApiDbContext Context { get; }
        int SaveChanges();
        int SaveChanges(SaveOptions options);
        void Dispose(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
        ObjectContext ObjectContext { get; }
        int DeleteStockByChallanId(long challanId);
        void BulkInsert<T>(List<T> entities, SqlRowsCopiedEventHandler callback = null);
        void BulkInsert<T>(List<T> entities, BulkInsertOptions options);
        void BulkInsert<T>(List<T> entities, IDbTransaction transaction);
        void BulkInsert<T>(List<T> entities, IDbTransaction transaction, BulkInsertOptions options);
        DbTransaction ODataBatchBeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool IsODataBatchContext { get; set; }
    }
}