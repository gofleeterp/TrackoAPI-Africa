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
    public static class APLLogAnxRepository
    {

        public static IQueryable<APLLogAnx> GetAllAPLLogAnxList(this IRepository<APLLogAnx> repository, long id) => repository.Queryable().Where(x => id == x.Id);

    }
}
