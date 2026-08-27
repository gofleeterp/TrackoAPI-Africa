using Service.Pattern;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.BMS;

namespace TrackoApi.Service
{
    public interface IMaterialDispatchOrderService : IService<MaterialDispatchOrder>
    {
        
    }
    public class MaterialDispatchOrderService : Service<MaterialDispatchOrder>, IMaterialDispatchOrderService
    {
        private readonly IRepositoryAsync<MaterialDispatchOrder> _repository;
        public MaterialDispatchOrderService(IRepositoryAsync<MaterialDispatchOrder> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
