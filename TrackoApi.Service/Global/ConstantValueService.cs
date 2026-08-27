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
    public interface IConstantValueService : IService<ConstantValue>
    {
        IQueryable<ConstantValue> GetAllConstantValueList(int id);
    }
    public class ConstantValueService : Service<ConstantValue>, IConstantValueService
    {
        private readonly IRepositoryAsync<ConstantValue> _repository;
        public ConstantValueService(IRepositoryAsync<ConstantValue> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<ConstantValue> GetAllConstantValueList(int brandid)
        {
            return _repository.GetAllConstantValueList(brandid);
        }
    }
}
