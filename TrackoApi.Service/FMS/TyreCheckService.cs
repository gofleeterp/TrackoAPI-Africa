using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ITyreCheckService : IService<TyreCheck>
    {
    }
    public class TyreCheckService : Service<TyreCheck>, ITyreCheckService
    {
        private readonly IRepositoryAsync<TyreCheck> _repository;
        public TyreCheckService(IRepositoryAsync<TyreCheck> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(TyreCheck entity)
        {
            if(_repository.GetRepository<TyreLog>().Queryable().Any(x=>x.TyreCheckId==entity.Id))throw new BusinessException(ErrorCode.GLB106,"Cannot Delete this Inspection Transaction as it was Created from Log.");
            base.Delete(entity);
        }
    }
}
