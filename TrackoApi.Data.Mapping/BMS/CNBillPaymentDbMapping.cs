using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class CNBillPaymentDbMapping : EntityTypeConfiguration<CNBillPayment>
    {
        public CNBillPaymentDbMapping()
        {
            HasRequired(x=>x.fk_Office).WithMany().HasForeignKey(x=>x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_BankCashAccount).WithMany().HasForeignKey(x => x.BankCashAccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_ClientAc).WithMany().HasForeignKey(x => x.ClientAcId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_PaymentMode).WithMany().HasForeignKey(x => x.PaymentModeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Other1Ac).WithMany().HasForeignKey(x => x.Other1AcId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Other2Ac).WithMany().HasForeignKey(x => x.Other2AcId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TDSLedgerAc).WithMany().HasForeignKey(x => x.TDSLedgerAcId).WillCascadeOnDelete(false);
            Ignore(x => x.BulkLog);
            Ignore(x => x.GenerateVoucherOnServer);
        }
    }
}
