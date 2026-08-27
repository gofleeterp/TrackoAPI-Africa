using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.FMS.GPS;

namespace TrackoAPI.Repository.FMS.GPS
{
    public static class GpsEndPointRepository
    {
        public static IQueryable<GpsEndPoint> GetGpsEndPointList(this IRepository<GpsEndPoint> repository,long id)
        {
            return repository.Queryable().Where(x => id == x.Id);
        }
    }
}
