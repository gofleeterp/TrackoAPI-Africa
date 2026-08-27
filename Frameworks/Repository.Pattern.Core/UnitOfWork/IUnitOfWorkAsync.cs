using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Threading;
using System.Threading.Tasks;
using EntityFramework.BulkInsert.Extensions;
using Repository.Pattern.Core.Repositories;
using System.Collections;
using System.Data;
using System.Linq;

namespace Repository.Pattern.Core.UnitOfWork
{
    public interface IUnitOfWorkAsync : IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(SaveOptions options);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        IRepositoryAsync<TEntity> RepositoryAsync<TEntity>() where TEntity : class;
        
        Task<DataTable> SqlQueryAsync(string sql, params object[] parameters);
        Task<int> ExecSqlQueryAsync(string sql,params object[] parameters);
        Task<DataSet> SqlQueryDataSetAsync(string sql, IDictionary<string, string> tableNameMapping = null, params object[] parameters);
        Task<string> SqlQueryAsJsonAsync(string sql, params object[] parameters);
        Task<int> ExecuteProcedureAsync(string sql, params object[] parameters);
    }
}