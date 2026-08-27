using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.CronJobs;

namespace TrackoAPI.Repository.Global
{
   public static class ScheduleLogRepository
    {
        public static IQueryable<ScheduleLog> GetAllScheduleLogList(this IRepository<ScheduleLog> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
