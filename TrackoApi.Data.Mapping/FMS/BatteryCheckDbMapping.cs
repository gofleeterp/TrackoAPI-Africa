using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class BatteryCheckDbMapping : EntityTypeConfiguration<BatteryCheck>
    {
        public BatteryCheckDbMapping()
        {
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Battery).WithMany().HasForeignKey(x => x.BatteryId).WillCascadeOnDelete(false);

        }
    }
}
