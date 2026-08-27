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
   public static class DriverRelativeRepository
    {
        public static IQueryable<DriverRelative> GetAllDriverRelativeList(this IRepository<DriverRelative> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
