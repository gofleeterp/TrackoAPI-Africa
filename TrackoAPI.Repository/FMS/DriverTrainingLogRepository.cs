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
   public static class DriverTrainingLogRepository
    {
        public static IQueryable<DriverTrainingLog> GetAllDriverTrainingLogList(this IRepository<DriverTrainingLog> repository,long id) => repository.Queryable().Where(x => id == x.Id);
    }
}
