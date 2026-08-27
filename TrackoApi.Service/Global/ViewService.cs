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
    public interface IViewService : IService<ApiView>
    {
    }
    public class ViewService : Service<ApiView>, IViewService
    {
        private readonly IRepositoryAsync<ApiView> _repository;
        public ViewService(IRepositoryAsync<ApiView> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
