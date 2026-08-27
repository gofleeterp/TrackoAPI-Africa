using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Repository
{
   public static class VehicleOwnerMappingRepository
    {
        public static IQueryable<VehicleOwnerMapping> GetAllVehicleOwnerMappingList(this IRepository<VehicleOwnerMapping> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
    }
}
