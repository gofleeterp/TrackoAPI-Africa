using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
    public static class APLLogAnxLevelRepository
    {

        public static IQueryable<APLLogAnxLevel> GetAllAPLLogAnxLevelList(this IRepository<APLLogAnxLevel> repository, long id) => repository.Queryable().Where(x => id == x.Id);

    }
}
