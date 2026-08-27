using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.GPS; 

namespace TrackoApi.Data.Mapping.FMS.GPS
{
    internal class GPSKmLogDbMapping : EntityTypeConfiguration<GPSKmLog>
    {
        public GPSKmLogDbMapping()
        {
            HasOptional(x=>x.fk_HireVehicle).WithMany().HasForeignKey(x=>x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
        }
    }
}