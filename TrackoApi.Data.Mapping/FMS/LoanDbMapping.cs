using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.Loan;

namespace TrackoApi.Data.Mapping
{
    internal class LoanDbMapping : EntityTypeConfiguration<Loan>
    {
        public LoanDbMapping()
        {
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x=>x.TypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Period).WithMany().HasForeignKey(z => z.PeriodId).WillCascadeOnDelete(false);
            HasMany(x => x.Logs).WithRequired(x => x.fk_Loan).HasForeignKey(x => x.LoanId).WillCascadeOnDelete(true);

            HasOptional(x => x.fk_CreditAc).WithMany().HasForeignKey(z => z.CreditAcId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DebitAc).WithMany().HasForeignKey(z => z.DebitAcId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TDSAc).WithMany().HasForeignKey(z => z.TDSAcId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_InterestAc).WithMany().HasForeignKey(z => z.InterestAcId).WillCascadeOnDelete(false);

        }
    }
}
