using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;

namespace TrackoApi.Service.TMS
{
    public interface IChallanService : IService<ChallanMaster>
    {
    }
    public class ChallanService : Service<ChallanMaster>, IChallanService
    {
        private readonly IRepositoryAsync<ChallanMaster> _repository;
        public ChallanService(IRepositoryAsync<ChallanMaster> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
