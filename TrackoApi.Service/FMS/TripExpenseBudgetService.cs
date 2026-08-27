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
    public interface ITripExpenseBudgetService : IService<TripExpenseBudget>
    {
    }
    public class TripExpenseBudgetService : Service<TripExpenseBudget>, ITripExpenseBudgetService
    {
        private readonly IRepositoryAsync<TripExpenseBudget> _repository;
        public TripExpenseBudgetService(IRepositoryAsync<TripExpenseBudget> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
