using System;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.CronJobs;
using TrackoAPI.Repository;
using TrackoAPI.Repository.Global;

namespace TrackoApi.Service.Global
{
    public interface IJobLogService : IService<JobLog>
    {
    }
    public class JobLogService : Service<JobLog>, IJobLogService
    {
        private readonly IRepositoryAsync<JobLog> _repository;
        public JobLogService(IRepositoryAsync<JobLog> repository) : base(repository)
        {
            _repository = repository;
        }
        

        public override JobLog Insert(JobLog entity)
        {
            return base.Insert(entity);
        }
        
    }
}
