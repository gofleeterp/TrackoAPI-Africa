using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ICNChallanService : IService<CnChallan>
    {
        IQueryable<CnChallan> GetAllCNChallanList(int id);
    }
    public class CNChallanService : Service<CnChallan>, ICNChallanService
    {
        private readonly IRepositoryAsync<CnChallan> _repository;
        public CNChallanService(IRepositoryAsync<CnChallan> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<CnChallan> GetAllCNChallanList(int brandid)
        {
            return _repository.GetAllCNChallanList(brandid);
        }
    }
}
