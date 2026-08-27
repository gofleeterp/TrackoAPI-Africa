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
    public interface IVehicleClassService : IService<VehicleClass>
    {
        IQueryable<VehicleClass> GetAllVehicleClassList(int id);
    }
    public class VehicleClassService : Service<VehicleClass>, IVehicleClassService
    {
        private readonly IRepositoryAsync<VehicleClass> _repository;
        public VehicleClassService(IRepositoryAsync<VehicleClass> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleClass> GetAllVehicleClassList(int brandid)
        {
            return _repository.GetAllVehicleClassList(brandid);
        }
    }
}
