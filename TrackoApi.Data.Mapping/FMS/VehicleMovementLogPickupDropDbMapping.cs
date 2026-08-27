using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleMovementLogPickupDropDbMapping : EntityTypeConfiguration<VehicleMovementLogPickupDrop>
    {
        public VehicleMovementLogPickupDropDbMapping()
        {
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_City).WithMany().HasForeignKey(x => x.CityId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Triplog).WithMany(x=>x.WayPoints).HasForeignKey(x => x.TriplogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripNature).WithMany().HasForeignKey(x => x.TripNatureId).WillCascadeOnDelete(false);
        }
    }
}
