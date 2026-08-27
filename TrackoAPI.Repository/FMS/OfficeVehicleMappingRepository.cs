using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository.FMS
{
    public static class OfficeVehicleMappingRepository
    {
        public static IQueryable<OfficeVehicleMapping> GetAllOfficeVehicleMappingList(this IRepository<OfficeVehicleMapping> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
