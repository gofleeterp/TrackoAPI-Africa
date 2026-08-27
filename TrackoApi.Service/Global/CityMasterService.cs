using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ICityMasterService : IService<CityMaster>
    {
        IQueryable<CityMaster> GetAllCityMasterList(int id);
    }
    public class CityMasterService : Service<CityMaster>, ICityMasterService
    {
        private readonly IRepositoryAsync<CityMaster> _repository;
        public CityMasterService(IRepositoryAsync<CityMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<CityMaster> GetAllCityMasterList(int brandid)
        {
            return _repository.GetAllCityMasterList(brandid);
        }
    }
}
