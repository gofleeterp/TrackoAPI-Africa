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
    public interface IVehicleAccidentClaimService : IService<VehicleAccidentClaim>
    {
        
    }
    public class VehicleAccidentClaimService : Service<VehicleAccidentClaim>, IVehicleAccidentClaimService
    {
        private readonly IRepositoryAsync<VehicleAccidentClaim> _repository;
        public VehicleAccidentClaimService(IRepositoryAsync<VehicleAccidentClaim> repository) : base(repository)
        {
            _repository = repository;
        }
        
        public override void Delete(VehicleAccidentClaim entity)
        {
            entity.ObjectState=ObjectState.Deleted;
            base.Delete(entity);
        }
    }
}
