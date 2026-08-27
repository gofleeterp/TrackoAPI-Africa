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
    public interface IDriverTrainingLogService : IService<DriverTrainingLog>
    {
        IQueryable<DriverTrainingLog> GetAllDriverTrainingLogList(int id);
    }
    public class DriverTrainingLogService : Service<DriverTrainingLog>, IDriverTrainingLogService
    {
        private readonly IRepositoryAsync<DriverTrainingLog> _repository;
        public DriverTrainingLogService(IRepositoryAsync<DriverTrainingLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<DriverTrainingLog> GetAllDriverTrainingLogList(int brandid)
        {
            return _repository.GetAllDriverTrainingLogList(brandid);
        }
    }
}
