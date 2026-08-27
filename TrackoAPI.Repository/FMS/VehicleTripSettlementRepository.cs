using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Reports.ViewModels.FMS.Driver;
using TrackoAPI.Reports.ViewModels.FMS.Repair;

namespace TrackoAPI.Repository
{
   public static class VehicleTripSettlementRepository
    {
        public static IQueryable<VehicleTripSettlement> GetAllVehicleTripSettlementList(this IRepository<VehicleTripSettlement> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
        
        
    }
}
