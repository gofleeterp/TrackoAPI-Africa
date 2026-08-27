using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.CronJobs;
using TrackoAPI.Repository;

namespace TrackoApi.Service.Global
{
    public interface IScheduleLogService : IService<ScheduleLog>
    {
    }
    public class ScheduleLogService : Service<ScheduleLog>, IScheduleLogService
    {
        private readonly IRepositoryAsync<ScheduleLog> _repository;
        public ScheduleLogService(IRepositoryAsync<ScheduleLog> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
