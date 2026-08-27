using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IFleetGatePassService : IService<FleetGatePass>
    {
    }
    public class FleetGatePassService : Service<FleetGatePass>, IFleetGatePassService
    {
        private readonly IRepositoryAsync<FleetGatePass> _repository;
        public FleetGatePassService(IRepositoryAsync<FleetGatePass> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(FleetGatePass entity)
        {
            if (_repository.Queryable().Any(x => x.Spares.Any(y=>y.StockQty!=y.Qty)||x.Batteries.Any(y=>y.NextLogId.HasValue)|| x.Tyres.Any(y => y.NextLogId.HasValue))) throw new BusinessException(ErrorCode.GLB106, "Cannot Delete this GatePass Transaction as Contained \nItems has been Reference in other Transactions.");
            base.Delete(entity);
        }
    }
}
