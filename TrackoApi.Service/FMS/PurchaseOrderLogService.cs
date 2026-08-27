using Service.Pattern;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS.Inventory;

namespace TrackoApi.Service
{
    public interface IPurchaseOrderLogService : IService<PurchaseOrderLog>
    {
        
    }
    public class PurchaseOrderLogService : Service<PurchaseOrderLog>, IPurchaseOrderLogService
    {
        private readonly IRepositoryAsync<PurchaseOrderLog> _repository;
        public PurchaseOrderLogService(IRepositoryAsync<PurchaseOrderLog> repository) : base(repository)
        {
            _repository = repository;
        }

    }
}
