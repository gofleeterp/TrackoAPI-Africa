using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IHireVehicleService : IService<HireVehicle>
    {
    }
    public class HireVehicleService : Service<HireVehicle>, IHireVehicleService
    {
        private readonly IRepositoryAsync<HireVehicle> _repository;
        public HireVehicleService(IRepositoryAsync<HireVehicle> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
