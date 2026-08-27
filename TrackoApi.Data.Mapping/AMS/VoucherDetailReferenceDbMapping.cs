using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.AMS;

namespace TrackoApi.Data.Mapping
{
    public class VoucherDetailReferenceDbMapping : EntityTypeConfiguration<VoucherDetailReference>
    {
        public VoucherDetailReferenceDbMapping()
        {
            HasRequired(x => x.fk_VoucherDetail).WithMany(x => x.VoucherDetailReferences).HasForeignKey(x => x.VoucherDetailId).WillCascadeOnDelete(true);
            //HasMany(x=>x.AgainstReferences).WithRequired(x=>x.fk_ParentReference).HasForeignKey(x=>x.RefId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_ParentReference).WithMany(x=>x.AgainstReferences).HasForeignKey(x=>x.RefId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_OriginalReference).WithMany().HasForeignKey(x => x.OriginalRefId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_ActualVD).WithMany().HasForeignKey(x => x.ActualVDId).WillCascadeOnDelete(false);
        }
    }
    public class VDRBalanceDbMapping : EntityTypeConfiguration<VDRBalance>
    {
        public VDRBalanceDbMapping()
        {
            HasTableAnnotation("IsView", "View");
            HasKey(x => x.VDRId);
            ToTable("View_VDRBalance");
        }
    }
}
