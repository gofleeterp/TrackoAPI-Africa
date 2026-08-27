using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.Inventory;

namespace TrackoApi.Data.Mapping
{
    internal class PurchaseOrderDbMapping : EntityTypeConfiguration<PurchaseOrder>
    {
        public PurchaseOrderDbMapping()
        {
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x=>x.TypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Vendor).WithMany().HasForeignKey(x => x.VendorId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_UsagePoint).WithMany().HasForeignKey(x => x.UsagePointId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Nature).WithMany().HasForeignKey(x => x.NatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CancelPO).WithMany().HasForeignKey(x => x.CancelPOId).WillCascadeOnDelete(false);

            HasMany(x => x.Logs).WithRequired(x => x.fk_PurchaseOrder).HasForeignKey(x => x.PurchaseOrderId).WillCascadeOnDelete(true);
            Ignore(x => x.DataView);
        }
    }
}
