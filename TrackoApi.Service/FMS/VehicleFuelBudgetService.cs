using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVehicleFuelBudgetService : IService<VehicleFuelBudget>
    {
        IQueryable<VehicleFuelBudget> GetAllVehicleFuelBudgetList(int id);
    }
    public class VehicleFuelBudgetService : Service<VehicleFuelBudget>, IVehicleFuelBudgetService
    {
        private readonly IRepositoryAsync<VehicleFuelBudget> _repository;
        public VehicleFuelBudgetService(IRepositoryAsync<VehicleFuelBudget> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleFuelBudget> GetAllVehicleFuelBudgetList(int brandid)
        {
            return _repository.GetAllVehicleFuelBudgetList(brandid);
        }
    }
}
