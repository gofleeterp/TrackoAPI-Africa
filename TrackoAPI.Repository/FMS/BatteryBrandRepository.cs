using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
    public static class BatteryBrandRepository
    {

        public static IQueryable<BatteryBrand> GetAllBrandList(this IRepository<BatteryBrand> repository,long id) => repository.Queryable().Where(x => id == x.Id);

        public static IQueryable<GenericMaster> GetAllGenericList(this IRepository<BatteryBrand> repository,
            long formid)
        {
            var iGm = repository.GetRepository<GenericMaster>();
            return iGm
                .Queryable()
                .Where(x => x.FormId == formid);
        }
    }

}
