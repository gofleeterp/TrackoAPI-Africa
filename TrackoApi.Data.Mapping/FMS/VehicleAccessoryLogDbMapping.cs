using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleAccessoryLogDbMapping : EntityTypeConfiguration<VehicleAccessoryLog>
    {
        public VehicleAccessoryLogDbMapping()
        {
            Ignore(x => x.Data);
            HasRequired(x => x.fk_Asset).WithMany().HasForeignKey(x => x.AssetId).WillCascadeOnDelete(false);
            HasOptional(X => X.fk_SpareLog).WithMany().HasForeignKey(X => X.SpareLogId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_Driver).WithMany().HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
        }
    }
}
