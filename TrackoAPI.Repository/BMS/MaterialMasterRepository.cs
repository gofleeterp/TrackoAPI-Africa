using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.BMS;

namespace TrackoAPI.Repository
{
   public static class MaterialMasterRepository
    {
        public static IQueryable<MaterialMaster> GetAllMaterialMasterList(this IRepository<MaterialMaster> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
