using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    internal class CNBillLogArchiveDbMapping : EntityTypeConfiguration<CNBillLogArchive>
    {
        public CNBillLogArchiveDbMapping()
        {
            HasOptional(x => x.fk_IGSTAC).WithMany().HasForeignKey(x => x.IGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAC).WithMany().HasForeignKey(x => x.SGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAC).WithMany().HasForeignKey(x => x.CGSTACId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Bill).WithMany().HasForeignKey(x => x.BillId).WillCascadeOnDelete(true);
        }
    }
}
