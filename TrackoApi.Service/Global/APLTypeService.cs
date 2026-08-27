using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IAPLTypeService : IService<APLType>
    {
        IQueryable<APLType> GetAllAPLTypeList(int id);
    }
    public class APLTypeService : Service<APLType>, IAPLTypeService
    {
        private readonly IRepositoryAsync<APLType> _repository;
        public APLTypeService(IRepositoryAsync<APLType> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<APLType> GetAllAPLTypeList(int brandid)
        {
            return _repository.GetAllAPLTypeList(brandid);
        }
    }
}
