using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class TyreLogDbMapping : EntityTypeConfiguration<TyreLog>
    {
        public TyreLogDbMapping()
        {
            HasOptional(x => x.fk_TSL).WithMany().HasForeignKey(x => x.TSLId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_IssueReceipt).WithMany().HasForeignKey(x => x.IssueReceiptId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_Reason).WithMany().HasForeignKey(x => x.ReasonId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RubberType).WithMany().HasForeignKey(x => x.RubberTypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Tyre).WithMany().HasForeignKey(x => x.TyreId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_TyreStatus).WithMany().HasForeignKey(x => x.TyreStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Jobsheet).WithMany().HasForeignKey(x => x.JobsheetId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_DebitAccount).WithMany().HasForeignKey(x=>x.DebitAccountId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_CreditAccount).WithMany().HasForeignKey(x=>x.CreditAccountId).WillCascadeOnDelete(false);
            Property(x => x.VoucherNo).IsRequired();
            HasRequired(x=>x.fk_VoucherType).WithMany().HasForeignKey(x=>x.VoucherTypeId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Tyre).WithMany().HasForeignKey(x=>x.TyreId).WillCascadeOnDelete(false);
            Property(x => x.TyreNo).IsRequired();
            Ignore(x => x.IgnoreValidation);
            HasOptional(x=>x.fk_NextLog).WithMany().HasForeignKey(x=>x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PreviousLog).WithMany().HasForeignKey(x=>x.PreviousLogId).WillCascadeOnDelete(false);
            HasOptional(x=>x.ExtraInfo).WithMany(x=>x.TyreLogs).HasForeignKey(x=>x.ExtraInfoId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TyreCheck).WithMany().HasForeignKey(x => x.TyreCheckId).WillCascadeOnDelete(false);
            Ignore(x => x.Data);
            //HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
        }
    }
    
}
