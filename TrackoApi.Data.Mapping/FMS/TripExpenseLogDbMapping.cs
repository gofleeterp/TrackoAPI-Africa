using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class TripExpenseLogDbMapping : EntityTypeConfiguration<TripExpenseLog>
    {
        public TripExpenseLogDbMapping()
        {
            HasRequired(x => x.fk_TripLog).WithMany().HasForeignKey(x=>x.TripLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Settlement).WithMany(x=>x.TripExpenses).HasForeignKey(x => x.SettlementId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_ExpenseType).WithMany().HasForeignKey(x=>x.ExpenseTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Draft).WithMany().HasForeignKey(x => x.DraftId).WillCascadeOnDelete(false);
        }
    }
}
