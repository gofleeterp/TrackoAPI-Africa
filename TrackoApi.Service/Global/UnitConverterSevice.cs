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
    public interface IUnitConverterService : IService<UnitConverter>
    {
        IQueryable<UnitConverter> GetAllUnitConverterList(int id);
    }
    public class UnitConverterService : Service<UnitConverter>, IUnitConverterService
    {
        private readonly IRepositoryAsync<UnitConverter> _repository;
        public UnitConverterService(IRepositoryAsync<UnitConverter> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<UnitConverter> GetAllUnitConverterList(int brandid)
        {
            return _repository.GetAllUnitConverterList(brandid);
        }
    }
}
