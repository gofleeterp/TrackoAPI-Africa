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
    public interface IConstantTypeService: IService<ConstantType>
    {
        IQueryable<ConstantType> GetAllDepricated();
    }

    public class ConstantTypeService : Service<ConstantType>, IConstantTypeService
    {
        private readonly IRepositoryAsync<ConstantType> _repository;
        public ConstantTypeService(IRepositoryAsync<ConstantType> repository) : base(repository)
        {
            _repository = repository;
        }
        

        public IQueryable<ConstantType> GetAllDepricated()
        {
            return _repository.GetAllDepricated();
        }
    }
}
