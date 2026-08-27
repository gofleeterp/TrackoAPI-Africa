using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
   public static class TripExpenseLogRepository
    {
        public static IQueryable<TripExpenseLog> GetAllTripExpenseLogList(this IRepository<TripExpenseLog> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
    }
}
