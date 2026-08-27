using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    internal class PurchaseRequisitionDbMapping : EntityTypeConfiguration<PurchaseRequisition>
    {
        public PurchaseRequisitionDbMapping()
        {
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Status).WithMany().HasForeignKey(x => x.StatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Store).WithMany().HasForeignKey(x => x.StoreId).WillCascadeOnDelete(false);
            Ignore(x => x.LogsJson);
            Ignore(x => x.DataView);
        }
    }
}
