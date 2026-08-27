using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ITripExpenseLogService : IService<TripExpenseLog>
    {
        IQueryable<TripExpenseLog> GetAllTripExpenseLogList(int id);
    }
    public class TripExpenseLogService : Service<TripExpenseLog>, ITripExpenseLogService
    {
        private readonly IRepositoryAsync<TripExpenseLog> _repository;
        public TripExpenseLogService(IRepositoryAsync<TripExpenseLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<TripExpenseLog> GetAllTripExpenseLogList(int brandid)
        {
            return _repository.GetAllTripExpenseLogList(brandid);
        }
    }
}
