using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.Global
{
    public class TripScheduleConfigDbMapping:EntityTypeConfiguration<TripScheduleConfiguration>
    {
        public TripScheduleConfigDbMapping()
        {
            Ignore(x => x.Cron);
        }
    }
}
