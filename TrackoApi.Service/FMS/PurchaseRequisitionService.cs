using Service.Pattern;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service
{
    public interface IPurchaseRequisitionService : IService<PurchaseRequisition>
    {
    }
    public class PurchaseRequisitionService : Service<PurchaseRequisition>, IPurchaseRequisitionService
    {
        private readonly IRepositoryAsync<PurchaseRequisition> _repository;
        public PurchaseRequisitionService(IRepositoryAsync<PurchaseRequisition> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
