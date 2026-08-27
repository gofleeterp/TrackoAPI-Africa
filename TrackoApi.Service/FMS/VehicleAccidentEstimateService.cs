using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVehicleAccidentEstimateService : IService<VehicleAccidentEstimate>
    {
        
    }
    public class VehicleAccidentEstimateService : Service<VehicleAccidentEstimate>, IVehicleAccidentEstimateService
    {
        private readonly IRepositoryAsync<VehicleAccidentEstimate> _repository;
        public VehicleAccidentEstimateService(IRepositoryAsync<VehicleAccidentEstimate> repository) : base(repository)
        {
            _repository = repository;
        }
        
        public override void Delete(VehicleAccidentEstimate entity)
        {
            entity.ObjectState=ObjectState.Deleted;
            base.Delete(entity);
        }
    }
}
