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
   public static class RouteTollMapRepository
    {
        public static IQueryable<RouteTollMap> GetAllRouteTollMapList(this IRepository<RouteTollMap> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
