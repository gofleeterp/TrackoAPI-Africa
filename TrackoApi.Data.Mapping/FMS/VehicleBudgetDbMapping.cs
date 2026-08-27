using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleBudgetDbMapping : EntityTypeConfiguration<VehicleBudget>
    {
        public VehicleBudgetDbMapping()
        {
            HasRequired(x => x.fk_Factor).WithMany().HasForeignKey(x=>x.CalculatingFactorId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Parameter).WithMany().HasForeignKey(x => x.CalculatingParameterId).WillCascadeOnDelete(false);

        }
    }
}
