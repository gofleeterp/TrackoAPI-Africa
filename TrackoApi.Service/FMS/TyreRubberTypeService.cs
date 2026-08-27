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
    public interface ITyreRubberTypeService : IService<TyreRubberType>
    {
        IQueryable<TyreRubberType> GetAllTyreRubberTypeList(int id);
    }
    public class TyreRubberTypeService : Service<TyreRubberType>, ITyreRubberTypeService
    {
        private readonly IRepositoryAsync<TyreRubberType> _repository;
        public TyreRubberTypeService(IRepositoryAsync<TyreRubberType> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<TyreRubberType> GetAllTyreRubberTypeList(int brandid)
        {
            return _repository.GetAllTyreRubberTypeList(brandid);
        }
    }
}
