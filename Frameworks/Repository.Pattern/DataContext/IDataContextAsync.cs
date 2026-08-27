using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using TrackoApi.Models.Global;

namespace Repository.Pattern.DataContext
{
    public interface IDataContextAsync : IDataContext
    {
        Task<int> ExecuteProcedureAsync(string sql, params object[] parameters);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
       
    }
}