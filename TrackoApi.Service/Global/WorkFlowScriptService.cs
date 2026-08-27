using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IWorkFlowScriptService : IService<ApiWorkFlowScript>
    {
    }
    public class WorkFlowScriptService : Service<ApiWorkFlowScript>, IWorkFlowScriptService
    {
        private readonly IRepositoryAsync<ApiWorkFlowScript> _repository;
        public WorkFlowScriptService(IRepositoryAsync<ApiWorkFlowScript> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
