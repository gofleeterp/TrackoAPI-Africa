using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class HSAdvanceDbMapping : EntityTypeConfiguration<HSAdvance>
    {
        public HSAdvanceDbMapping()
        {
            HasRequired(x=>x.fk_CreditAccount).WithMany().HasForeignKey(x => x.CrAccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_DreditAccount).WithMany().HasForeignKey(x => x.DrAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PaymentMode).WithMany().HasForeignKey(x => x.PaymentModeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RefHSAdvance).WithMany(x => x.Settlements).HasForeignKey(x => x.RefAdvId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Voucher).WithMany().HasForeignKey(x=>x.VoucherId).WillCascadeOnDelete(true);
        }
    }
}
