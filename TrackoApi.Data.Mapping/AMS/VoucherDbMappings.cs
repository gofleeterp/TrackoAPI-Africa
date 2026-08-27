using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.AMS;

namespace TrackoApi.Data.Mapping
{
    public class VoucherDbMappings : EntityTypeConfiguration<Voucher>
    {
        public VoucherDbMappings()
        {
            HasRequired(x => x.FK_VoucherType).WithMany().HasForeignKey(x => x.VoucherTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_FinancialYear).WithMany().HasForeignKey(x => x.FinancialYearId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.Account1).WithMany().HasForeignKey(x => x.Account1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account2).WithMany().HasForeignKey(x => x.Account2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account3).WithMany().HasForeignKey(x => x.Account3Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account4).WithMany().HasForeignKey(x => x.Account4Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account5).WithMany().HasForeignKey(x => x.Account5Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account6).WithMany().HasForeignKey(x => x.Account6Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account7).WithMany().HasForeignKey(x => x.Account7Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account8).WithMany().HasForeignKey(x => x.Account8Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account9).WithMany().HasForeignKey(x => x.Account9Id).WillCascadeOnDelete(false);
            HasOptional(x => x.Account10).WithMany().HasForeignKey(x => x.Account10Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Parent).WithMany(x => x.ChildVouchers).HasForeignKey(x => x.ParentId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GroupVoucher).WithMany().HasForeignKey(x => x.GroupVoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GSTChallanVoucher).WithMany().HasForeignKey(x => x.GSTChallanVoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GSTR2AUploadUser).WithMany().HasForeignKey(x => x.GSTR2AUploadUserId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GSTR1FinalUser).WithMany().HasForeignKey(x => x.GSTR1FinalUserId).WillCascadeOnDelete(false);


            Ignore(x => x.RemoveVD);
            Ignore(x => x.VdrJson);
            Ignore(x => x.Data);
        }
    }

    public class VoucherDetailDbMapping : EntityTypeConfiguration<VoucherDetail>
    {
        public VoucherDetailDbMapping()
        {
            HasRequired(x => x.Voucher).WithMany(x => x.VoucherDetails).HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Account).WithMany().HasForeignKey(x => x.AccountId).WillCascadeOnDelete(false);
            Ignore(x => x.JsonVDRS);
        }
    }


}
