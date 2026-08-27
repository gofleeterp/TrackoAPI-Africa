using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IRouteVehicleTypeService : IService<RouteVehicleType>
    {
        
    }
    public class RouteVehicleTypeService : Service<RouteVehicleType>, IRouteVehicleTypeService
    {
        private readonly IRepositoryAsync<RouteVehicleType> _repository;
        public RouteVehicleTypeService(IRepositoryAsync<RouteVehicleType> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
