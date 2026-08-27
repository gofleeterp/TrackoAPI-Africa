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
    public interface IRouteTollMapService : IService<RouteTollMap>
    {
        IQueryable<RouteTollMap> GetAllRouteTollMapList(int id);
    }
    public class RouteTollMapService : Service<RouteTollMap>, IRouteTollMapService
    {
        private readonly IRepositoryAsync<RouteTollMap> _repository;
        public RouteTollMapService(IRepositoryAsync<RouteTollMap> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<RouteTollMap> GetAllRouteTollMapList(int brandid)
        {
            return _repository.GetAllRouteTollMapList(brandid);
        }
    }
}
