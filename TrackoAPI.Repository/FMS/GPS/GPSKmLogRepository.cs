using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS.GPS;

namespace TrackoAPI.Repository.FMS.GPS
{
    public static class GPSKmLogRepository
    {
        public static IQueryable<GPSKmLog> GetGPSKmLogList(this IRepository<GPSKmLog> repository,long id)
        {
            return repository.Queryable().Where(x => id == x.Id);
        }
    }
}
