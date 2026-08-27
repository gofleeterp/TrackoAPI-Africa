using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Repository
{
    public static class VehicleMovementLogPickupDropRepository
    {
        public static IQueryable<VehicleMovementLogPickupDrop> GetAllVehicleMovementLogPickupDropList(this IRepository<VehicleMovementLogPickupDrop> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
        
        
    }
}
