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
    public interface IVehicleDueMappingService : IService<VehicleDueMapping>
    {
        IQueryable<VehicleDueMapping> GetAllVehicleDueMappingList(int id);
    }
    public class VehicleDueMappingService : Service<VehicleDueMapping>, IVehicleDueMappingService
    {
        private readonly IRepositoryAsync<VehicleDueMapping> _repository;
        public VehicleDueMappingService(IRepositoryAsync<VehicleDueMapping> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleDueMapping> GetAllVehicleDueMappingList(int brandid)
        {
            return _repository.GetAllVehicleDueMappingList(brandid);
        }
    }
}
