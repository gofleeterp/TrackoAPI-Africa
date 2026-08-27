using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.Loan;

namespace TrackoApi.Data.Mapping
{
    internal class LoanLogDbMapping : EntityTypeConfiguration<LoanLog>
    {
        public LoanLogDbMapping()
        {
            HasRequired(x => x.fk_Loan).WithMany().HasForeignKey(x=>x.LoanId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ParentLogs).WithMany().HasForeignKey(x => x.ParentLogId).WillCascadeOnDelete(false);
            HasOptional(x=> x.fk_LoanVoucher).WithMany().HasForeignKey(x=>x.LoanVoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RepVoucher).WithMany().HasForeignKey(x => x.RepVoucherId).WillCascadeOnDelete(false);
           
        }
    }
}
