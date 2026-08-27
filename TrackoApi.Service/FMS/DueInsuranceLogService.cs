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
    public interface IDueInsuranceLogService : IService<DueInsuranceLog>
    {
        IQueryable<DueInsuranceLog> GetAllDueInsuranceLogList(int id);
    }
    public class DueInsuranceLogService : Service<DueInsuranceLog>, IDueInsuranceLogService
    {
        private readonly IRepositoryAsync<DueInsuranceLog> _repository;
        public DueInsuranceLogService(IRepositoryAsync<DueInsuranceLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<DueInsuranceLog> GetAllDueInsuranceLogList(int brandid)
        {
            return _repository.GetAllDueInsuranceLogList(brandid);
        }
    }
}
