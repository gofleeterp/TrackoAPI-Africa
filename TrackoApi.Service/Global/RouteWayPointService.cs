using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IRouteWayPointService : IService<RouteWayPoint>
    {
    }
    public class RouteWayPointService : Service<RouteWayPoint>, IRouteWayPointService
    {
        private readonly IRepositoryAsync<RouteWayPoint> _repository;
        public RouteWayPointService(IRepositoryAsync<RouteWayPoint> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
