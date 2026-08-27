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
    public interface IDriverIncidentLogService : IService<DriverIncidentLog>
    {
        IQueryable<DriverIncidentLog> GetAllDriverEventLogList(int driverid);
    }
    public class DriverIncidentLogService : Service<DriverIncidentLog>, IDriverIncidentLogService
    {
        private readonly IRepositoryAsync<DriverIncidentLog> _repository;
        public DriverIncidentLogService(IRepositoryAsync<DriverIncidentLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<DriverIncidentLog> GetAllDriverEventLogList(int driverid)
        {
            return _repository.GetAllDriverIncidentLogs(driverid);
        }
    }
}
