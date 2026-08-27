using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.CronJobs;

namespace TrackoAPI.Repository.Global
{
   public static class JobLogRepository
    {
        public static IQueryable<JobLog> GetAllJobLogList(this IRepository<JobLog> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
