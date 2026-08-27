using System.Linq;

using Repository.Pattern.Core.Repositories;

using TrackoApi.Models.Global;

namespace TrackoAPI.Repository
{
    public static class APLConfigRepository
    {
        public static IQueryable<APLConfig> GetAllAPLConfigList(this IRepository<APLConfig> repository, long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
