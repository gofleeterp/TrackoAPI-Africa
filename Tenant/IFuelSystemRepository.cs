using System.Threading.Tasks;

namespace Tenant
{
    public interface IFuelSystemRepository
    {
        Task FetchIOCRate();
        Task SyncTolls();
    }
}