using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Tyres;

namespace TrackoApi.Data.Mapping
{
    public class TyreMasterDbMapping : EntityTypeConfiguration<TyreMaster>
    {
        public TyreMasterDbMapping()
        {
            HasRequired(x => x.fk_Brand).WithMany().HasForeignKey(x=>x.BrandId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PurchaseVoucher).WithMany().HasForeignKey(x=>x.PurchaseVoucherId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PurchaseTyreLog).WithMany().HasForeignKey(x=>x.PurchaseLogId).WillCascadeOnDelete(true);
            HasRequired(x=>x.fk_S_OtherAccount).WithMany().HasForeignKey(x=>x.S_CreditAccountId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_S_DebitAccount).WithMany().HasForeignKey(x=>x.S_DebitAccountId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_S_Voucher).WithMany().HasForeignKey(x=>x.S_VoucherId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_S_TyreLog).WithMany().HasForeignKey(x=>x.S_TyreLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
            //HasMany(x => x.TyreMillageLogs).WithRequired(x => x.fk_Tyre).HasForeignKey(x => x.TyreId).WillCascadeOnDelete(true);
        }
    }

    public class TyreLogExtraInfoDbMapping : EntityTypeConfiguration<TyreLogExtraInfo>
    {
        public TyreLogExtraInfoDbMapping()
        {
            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(true);
            HasOptional(x=>x.fk_ORM).WithMany().HasForeignKey(x=>x.ORMId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasKey(x => x.Id);
            HasOptional(x => x.fk_IGSTAC).WithMany().HasForeignKey(x => x.IGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAC).WithMany().HasForeignKey(x => x.SGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAC).WithMany().HasForeignKey(x => x.CGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PartyGSTOffice).WithMany().HasForeignKey(x => x.PartyGSTOfficeId).WillCascadeOnDelete(false);

            Ignore(x => x.Data);
        }
    }
    //
    public class TyreLifePerformanceDbMapping : EntityTypeConfiguration<TyreLifePerformanceLog>
    {
        public TyreLifePerformanceDbMapping()
        {
            HasRequired(x => x.fk_Tyre).WithMany().HasForeignKey(x => x.TyreId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_FirstIssueLog).WithMany().HasForeignKey(x => x.FirstIssueLogId).WillCascadeOnDelete(true);
            HasKey(x => x.Id);
        }
    }

    public class TyreMillageLogDbMapping : EntityTypeConfiguration<TyreMillageLog>
    {
        public TyreMillageLogDbMapping()
        {
            HasRequired(x => x.fk_Tyre).WithMany(x => x.TyreMillageLogs).HasForeignKey(x => x.TyreId).WillCascadeOnDelete(false);

        }
    }
}
