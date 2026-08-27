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
    public interface IGenericMasterService : IService<GenericMaster>
    {
        IQueryable<GenericMaster> GetAllGenericMasterList(int id);
    }
    public class GenericMasterService : Service<GenericMaster>, IGenericMasterService
    {
        private readonly IRepositoryAsync<GenericMaster> _repository;
        public GenericMasterService(IRepositoryAsync<GenericMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<GenericMaster> GetAllGenericMasterList(int brandid)
        {
            return _repository.GetAllGenericMasterList(brandid);
        }
    }
}
