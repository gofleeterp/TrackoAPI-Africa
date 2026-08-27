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
    public interface ITollMasterService : IService<TollMaster>
    {
        IQueryable<TollMaster> GetAllTollMasterList(int id);
    }
    public class TollMasterService : Service<TollMaster>, ITollMasterService
    {
        private readonly IRepositoryAsync<TollMaster> _repository;
        public TollMasterService(IRepositoryAsync<TollMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<TollMaster> GetAllTollMasterList(int brandid)
        {
            return _repository.GetAllTollMasterList(brandid);
        }
    }
}
