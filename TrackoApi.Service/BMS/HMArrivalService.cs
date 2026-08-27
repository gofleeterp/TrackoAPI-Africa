using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoApi.Service.BMS
{
    public interface IHMArrivalService : IService<HMArrival>
    {
    }
    public class HMArrivalService : Service<HMArrival>, IHMArrivalService
    {
        private readonly IRepositoryAsync<HMArrival> _repository;
        public HMArrivalService(IRepositoryAsync<HMArrival> repository) : base(repository)
        {
            _repository = repository;
        }
    }
    public interface IHMArrivalLogService : IService<HMArrivalLog>
    {
    }
    public class HMArrivalLogService : Service<HMArrivalLog>, IHMArrivalLogService
    {
        private readonly IRepositoryAsync<HMArrivalLog> _repository;
        public HMArrivalLogService(IRepositoryAsync<HMArrivalLog> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
