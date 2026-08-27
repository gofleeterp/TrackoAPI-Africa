using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class BatteryLogDbMapping : EntityTypeConfiguration<BatteryLog>
    {
        public BatteryLogDbMapping()
        {
            HasOptional(x => x.fk_TSL).WithMany().HasForeignKey(x => x.TSLId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_IssueReceipt).WithMany().HasForeignKey(x => x.IssueReceiptId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_Reason).WithMany().HasForeignKey(x => x.ReasonId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_RubberType).WithMany().HasForeignKey(x => x.RubberTypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Battery).WithMany().HasForeignKey(x => x.BatteryId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_BatteryStatus).WithMany().HasForeignKey(x => x.BatteryStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Jobsheet).WithMany().HasForeignKey(x => x.JobsheetId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(true);
            HasRequired(x=>x.fk_DebitAccount).WithMany().HasForeignKey(x=>x.DebitAccountId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_CreditAccount).WithMany().HasForeignKey(x=>x.CreditAccountId).WillCascadeOnDelete(false);
            Property(x => x.DocNo).IsRequired();
            HasRequired(x=>x.fk_VoucherType).WithMany().HasForeignKey(x=>x.VoucherTypeId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Battery).WithMany().HasForeignKey(x=>x.BatteryId).WillCascadeOnDelete(false);
            Property(x => x.BatterySerialNo).IsRequired();
            Ignore(x => x.IgnoreValidation);
            HasOptional(x=>x.fk_NextLog).WithMany().HasForeignKey(x=>x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PreviousLog).WithMany().HasForeignKey(x=>x.PreviousLogId).WillCascadeOnDelete(false);
            HasOptional(x=>x.ExtraInfo).WithMany(x=>x.BatteryLogs).HasForeignKey(x=>x.ExtraInfoId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
            Ignore(x => x.Data);
        }
    }
    
}
