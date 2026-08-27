using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleDueMappingDbMapping : EntityTypeConfiguration<VehicleDueMapping>
    {
        public VehicleDueMappingDbMapping()
        {
            HasRequired(x => x.fk_Vehicle).WithMany(x=>x.Dues).HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Due).WithMany().HasForeignKey(x => x.DueId).WillCascadeOnDelete(true);
        }
    }
}
