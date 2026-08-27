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
    public interface IRouteCityMapService : IService<RouteCityMap>
    {
        IQueryable<RouteCityMap> GetAllRouteCityMapList(int id);
    }
    public class RouteCityMapService : Service<RouteCityMap>, IRouteCityMapService
    {
        private readonly IRepositoryAsync<RouteCityMap> _repository;
        public RouteCityMapService(IRepositoryAsync<RouteCityMap> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<RouteCityMap> GetAllRouteCityMapList(int brandid)
        {
            return _repository.GetAllRouteCityMapList(brandid);
        }
    }
}
