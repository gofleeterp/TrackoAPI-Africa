using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Repository
{
   public static class BatteryCheckRepository
    {
        public static IQueryable<BatteryCheck> GetAllBatteryCheckList(this IRepository<BatteryCheck> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
    }
}
