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
    public static class ConstantTypeRepository
    {

        public static IQueryable<ConstantType> GetAllDepricated(this IRepository<ConstantType> repository) => repository.Queryable().Where(x => x.IsDepricated);

    }
}
