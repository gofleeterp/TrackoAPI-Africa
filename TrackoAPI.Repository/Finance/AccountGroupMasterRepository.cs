using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
    public static class AccountGroupRepository
    {
        public static IQueryable<AccountGroup> GetAllAccountGroupList(this IRepository<AccountGroup> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
