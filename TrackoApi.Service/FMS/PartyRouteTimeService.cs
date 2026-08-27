using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service.FMS
{
    public interface IPartyRouteTimeService : IService<PartyRouteTime>
    {
        
    }
    public class PartyRouteTimeService : Service<PartyRouteTime>, IPartyRouteTimeService
    {
        private readonly IRepositoryAsync<PartyRouteTime> _repository;
        public PartyRouteTimeService(IRepositoryAsync<PartyRouteTime> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
