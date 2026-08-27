using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IHSAdvanceService : IService<HSAdvance>
    {
    }
    public class HSAdvanceService : Service<HSAdvance>, IHSAdvanceService
    {
        private readonly IRepositoryAsync<HSAdvance> _repository;
        public HSAdvanceService(IRepositoryAsync<HSAdvance> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
