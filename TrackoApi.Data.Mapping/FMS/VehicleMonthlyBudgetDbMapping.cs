using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleMonthlyBudgetDbMapping : EntityTypeConfiguration<VehicleMonthlyBudget>
    {
        public VehicleMonthlyBudgetDbMapping()
        {
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
           

        }
    }
}
