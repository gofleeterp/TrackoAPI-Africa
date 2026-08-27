using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IMaterialMasterService : IService<MaterialMaster>
    {
        IQueryable<MaterialMaster> GetAllMaterialMasterList(int id);
    }
    public class MaterialMasterService : Service<MaterialMaster>, IMaterialMasterService
    {
        private readonly IRepositoryAsync<MaterialMaster> _repository;
        public MaterialMasterService(IRepositoryAsync<MaterialMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<MaterialMaster> GetAllMaterialMasterList(int brandid)
        {
            return _repository.GetAllMaterialMasterList(brandid);
        }
    }
}
