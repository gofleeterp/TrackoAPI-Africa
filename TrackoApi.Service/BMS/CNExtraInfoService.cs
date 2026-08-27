using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service.TMS
{
    public interface ICNExtraInfoService : IService<CNExtraInfo>
    {
    }
    public class CNExtraInfoService : Service<CNExtraInfo>, ICNExtraInfoService
    {
        private readonly IRepositoryAsync<CNExtraInfo> _repository;
        public CNExtraInfoService(IRepositoryAsync<CNExtraInfo> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
