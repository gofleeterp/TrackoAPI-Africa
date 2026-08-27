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
    public interface IVehicleMonthlyBudgetService : IService<VehicleMonthlyBudget>
    {
        
    }
    public class VehicleMonthlyBudgetService : Service<VehicleMonthlyBudget>, IVehicleMonthlyBudgetService
    {
        private readonly IRepositoryAsync<VehicleMonthlyBudget> _repository;
        public VehicleMonthlyBudgetService(IRepositoryAsync<VehicleMonthlyBudget> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
