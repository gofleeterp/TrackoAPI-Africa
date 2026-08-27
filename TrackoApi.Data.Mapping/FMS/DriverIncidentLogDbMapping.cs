using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    internal class DriverIncidentLogDbMapping : EntityTypeConfiguration<DriverIncidentLog>
    {
        public DriverIncidentLogDbMapping()
        {
            HasRequired(x => x.fk_Driver).WithMany(x=>x.IncidentLogs).HasForeignKey(x=>x.DriverId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Vehicle).WithMany().HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
        }
    }
}
