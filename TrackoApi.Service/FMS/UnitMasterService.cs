using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service
{
    public interface IUnitMasterService:IService<UnitMaster>
    {
    }

    public class UnitMasterService : Service<UnitMaster>, IUnitMasterService
    {
        private IRepository<UnitMaster> _repo;
        public UnitMasterService(IRepositoryAsync<UnitMaster> repository) : base(repository)
        {
            _repo = repository;
        }
    }
}
