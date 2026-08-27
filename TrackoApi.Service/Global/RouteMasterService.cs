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
    public interface IRouteMasterService : IService<RouteMaster>
    {
        IQueryable<RouteMaster> GetAllRouteMasterList(int id);
    }
    public class RouteMasterService : Service<RouteMaster>, IRouteMasterService
    {
        private readonly IRepositoryAsync<RouteMaster> _repository;
        public RouteMasterService(IRepositoryAsync<RouteMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<RouteMaster> GetAllRouteMasterList(int brandid)
        {
            return _repository.GetAllRouteMasterList(brandid);
        }
    }
}
