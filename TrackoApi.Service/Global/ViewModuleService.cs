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
    public interface IViewModuleService : IService<ApiViewModule>
    {
    }
    public class ViewModuleService : Service<ApiViewModule>, IViewModuleService
    {
        private readonly IRepositoryAsync<ApiViewModule> _repository;
        public ViewModuleService(IRepositoryAsync<ApiViewModule> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
