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
   public static class VehicleFuelBudgetRepository
    {
        public static IQueryable<VehicleFuelBudget> GetAllVehicleFuelBudgetList(this IRepository<VehicleFuelBudget> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
    }
}
