using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class TripExpenseBudgetDbMapping : EntityTypeConfiguration<TripExpenseBudget>
    {
        public TripExpenseBudgetDbMapping()
        {
            HasRequired(x => x.fk_ExpenseType).WithMany().HasForeignKey(x=>x.ExpenseTypeId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_Route).WithMany().HasForeignKey(x => x.RouteId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_VehicleType).WithMany().HasForeignKey(x => x.VehicleTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Script).WithMany().HasForeignKey(x => x.ScriptId).WillCascadeOnDelete(false);
        }
    }
}
