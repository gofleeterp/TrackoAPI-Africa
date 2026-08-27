using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleOwnerDbMapping:EntityTypeConfiguration<VehicleOwnerMapping>
    {
        public VehicleOwnerDbMapping()
        {
            HasRequired(x=>x.fk_Vehicle).WithMany().HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Owner).WithMany().HasForeignKey(x=>x.OwnerId).WillCascadeOnDelete(false);
        }
    }
}
