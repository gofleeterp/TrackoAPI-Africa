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
    public interface ITyreMasterService : IService<TyreMaster>
    {
        IQueryable<TyreMaster> GetAllTyreMasterList(int id);
    }
    public class TyreMasterService : Service<TyreMaster>, ITyreMasterService
    {
        private readonly IRepositoryAsync<TyreMaster> _repository;
        public TyreMasterService(IRepositoryAsync<TyreMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<TyreMaster> GetAllTyreMasterList(int brandid)
        {
            return _repository.GetAllTyreMasterList(brandid);
        }
    }
}
