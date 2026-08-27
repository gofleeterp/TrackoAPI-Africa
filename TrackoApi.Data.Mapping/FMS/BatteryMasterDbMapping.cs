using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class BatteryMasterDbMapping : EntityTypeConfiguration<BatteryMaster>
    {
        public BatteryMasterDbMapping()
        {
            HasRequired(x => x.fk_Brand).WithMany().HasForeignKey(x=>x.BrandId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PurchaseExtraInfo).WithMany().HasForeignKey(x=>x.PurchaseExtraInfoId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PurchaseBatteryLog).WithMany().HasForeignKey(x=>x.PurchaseLogId).WillCascadeOnDelete(true);
            HasRequired(x=>x.fk_S_OtherAccount).WithMany().HasForeignKey(x=>x.S_CreditAccountId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_S_DebitAccount).WithMany().HasForeignKey(x=>x.S_DebitAccountId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_S_ExtraInfo).WithMany().HasForeignKey(x=>x.S_ExtraInfoId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_S_BatteryLog).WithMany().HasForeignKey(x=>x.S_BatteryLogId).WillCascadeOnDelete(false);
            
        }
    }

    public class BatteryLogExtraInfoDbMapping : EntityTypeConfiguration<BatteryLogExtraInfo>
    {
        public BatteryLogExtraInfoDbMapping()
        {
            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ORM).WithMany().HasForeignKey(x => x.ORMId).WillCascadeOnDelete(false);
            HasKey(x => x.Id);
            HasOptional(x => x.fk_IGSTAC).WithMany().HasForeignKey(x => x.IGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAC).WithMany().HasForeignKey(x => x.SGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAC).WithMany().HasForeignKey(x => x.CGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PartyGSTOffice).WithMany().HasForeignKey(x => x.PartyGSTOfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CurType).WithMany().HasForeignKey(x => x.CurTypeId).WillCascadeOnDelete(false);

            Ignore(x => x.Data);
        }
    }
    //
    public class BatteryLifePerformanceDbMapping : EntityTypeConfiguration<BatteryLifePerformanceLog>
    {
        public BatteryLifePerformanceDbMapping()
        {
            HasRequired(x => x.fk_Battery).WithMany().HasForeignKey(x => x.BatteryId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_FirstIssueLog).WithMany().HasForeignKey(x => x.FirstIssueLogId).WillCascadeOnDelete(true);
            HasKey(x => x.Id);
            Ignore(x => x.Data);
        }
    }
}
