
using System;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;
using TrackoAPI.Repository.BMS;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Service
{
    public interface ICNStockMMLogService : IService<CNStockMMLog>
    {
       // IQueryable<vwCNStockSearch> GetTop10CnStock(long challanOfficeId, long stockOfficeId, DateTime stockDate, string searchTerm);
        
    }
    public class CNStockMMLogService : Service<CNStockMMLog>, ICNStockMMLogService
    {
        private readonly IRepositoryAsync<CNStockMMLog> _repository;
        public CNStockMMLogService(IRepositoryAsync<CNStockMMLog> repository) : base(repository)
        {
            _repository = repository;
        }

        //public IQueryable<vwCNStockSearch> GetTop10CnStock(long challanOfficeId, long stockOfficeId, DateTime stockDate, string searchTerm)
        //{
        //    return _repository.GetTop10CnStock(challanOfficeId, stockOfficeId, stockDate, searchTerm);
        //}
        public override CNStockMMLog Insert(CNStockMMLog entity)
        {
            return base.Insert(entity);
        }
    }
}
