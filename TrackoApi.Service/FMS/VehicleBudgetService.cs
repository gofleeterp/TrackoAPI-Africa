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
    public interface IVehicleBudgetService : IService<VehicleBudget>
    {
        IQueryable<VehicleBudget> GetAllVehicleBudgetList(int id);
    }
    public class VehicleBudgetService : Service<VehicleBudget>, IVehicleBudgetService
    {
        private readonly IRepositoryAsync<VehicleBudget> _repository;
        public VehicleBudgetService(IRepositoryAsync<VehicleBudget> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleBudget> GetAllVehicleBudgetList(int brandid)
        {
            return _repository.GetAllVehicleBudgetList(brandid);
        }
    }
}
