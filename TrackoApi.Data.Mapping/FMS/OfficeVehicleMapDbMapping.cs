using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class OfficeVehicleMapDbMapping : EntityTypeConfiguration<OfficeVehicleMap>
    {
        public OfficeVehicleMapDbMapping()
        {
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
        }
    }
}
