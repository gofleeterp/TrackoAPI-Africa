using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleTrailorDbMapping : EntityTypeConfiguration<VehicleTrailorMapping>
    {
        public VehicleTrailorDbMapping()
        {
            HasRequired(x => x.fk_Vehicle).WithMany(x => x.Trailors).HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Trailor).WithMany(x => x.Vehicles).HasForeignKey(x => x.TrailorId).WillCascadeOnDelete(false);
            Property(x => x.OnDate).IsRequired();
            Property(x => x.OffDate).IsOptional();
        }
    }
}
