using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;
using TrackoAPI.Repository.BMS;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Service
{
    public interface ICNStockLogService : IService<CNStockLog>
    {
        IQueryable<vwCNStockSearch> GetTop10CnStock(long challanOfficeId, long stockOfficeId, DateTime stockDate, string searchTerm);
    }
    public class CNStockLogService : Service<CNStockLog>, ICNStockLogService
    {
        private readonly IRepositoryAsync<CNStockLog> _repository;
        public CNStockLogService(IRepositoryAsync<CNStockLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<vwCNStockSearch> GetTop10CnStock(long challanOfficeId, long stockOfficeId, DateTime stockDate, string searchTerm)
        {
            return _repository.GetTop10CnStock(challanOfficeId, stockOfficeId, stockDate, searchTerm);
        }
    }
}
