using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class CNBillPaymentLogDbMapping : EntityTypeConfiguration<CNBillPaymentLog>
    {
        public CNBillPaymentLogDbMapping()
        {
            HasRequired(x=>x.fk_Office).WithMany().HasForeignKey(x=>x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Account).WithMany().HasForeignKey(x => x.AccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Payment).WithMany(x=>x.PaymentLogs).HasForeignKey(x => x.PaymentId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Bill).WithMany().HasForeignKey(x => x.BillId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CN).WithMany().HasForeignKey(x => x.CNId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_OnAccountRef).WithMany(x=>x.OnAcSettlements).HasForeignKey(x => x.OnAccountRefId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_VDR).WithMany().HasForeignKey(x => x.VDRId).WillCascadeOnDelete(false);
            
        }
    }
}
