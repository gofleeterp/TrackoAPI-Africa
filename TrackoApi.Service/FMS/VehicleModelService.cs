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
    public interface IVehicleModelService : IService<VehicleModel>
    {
        IQueryable<VehicleModel> GetAllVehicleModelList(int id);
    }
    public class VehicleModelService : Service<VehicleModel>, IVehicleModelService
    {
        private readonly IRepositoryAsync<VehicleModel> _repository;
        public VehicleModelService(IRepositoryAsync<VehicleModel> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleModel> GetAllVehicleModelList(int brandid)
        {
            return _repository.GetAllVehicleModelList(brandid);
        }
    }
}
