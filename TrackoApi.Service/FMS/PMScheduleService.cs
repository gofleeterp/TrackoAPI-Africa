using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IPMScheduleService : IService<PMSchedule>
    {
    }
    public class PMScheduleService : Service<PMSchedule>, IPMScheduleService
    {
        private readonly IRepositoryAsync<PMSchedule> _repository;
        public PMScheduleService(IRepositoryAsync<PMSchedule> repository) : base(repository)
        {
            _repository = repository;
        }

        public override PMSchedule Insert(PMSchedule entity)
        {
            return base.Insert(entity);
        }
    }
}
