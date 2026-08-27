using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleFuelBudgetDbMapping : EntityTypeConfiguration<VehicleFuelBudget>
    {
        public VehicleFuelBudgetDbMapping()
        {
            HasRequired(x => x.fk_ObjectClass).WithMany().HasForeignKey(x=>x.ObjectClassId).WillCascadeOnDelete(false);

        }
    }
}
