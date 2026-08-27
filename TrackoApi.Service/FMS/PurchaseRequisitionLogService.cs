using Service.Pattern;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service
{
    public interface IPurchaseRequisitionLogService : IService<PurchaseRequisitionLog>
    {
        
    }
    public class PurchaseRequisitionLogService : Service<PurchaseRequisitionLog>, IPurchaseRequisitionLogService
    {
        private readonly IRepositoryAsync<PurchaseRequisitionLog> _repository;
        public PurchaseRequisitionLogService(IRepositoryAsync<PurchaseRequisitionLog> repository) : base(repository)
        {
            _repository = repository;
        }

    }
}
