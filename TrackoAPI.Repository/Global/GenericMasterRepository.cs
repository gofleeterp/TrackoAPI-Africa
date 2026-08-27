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
   public static class GenericMasterRepository
    {
        public static IQueryable<GenericMaster> GetAllGenericMasterList(this IRepository<GenericMaster> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
