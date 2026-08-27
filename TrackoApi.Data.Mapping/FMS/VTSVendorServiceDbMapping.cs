using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VTSVendorServiceDbMapping : EntityTypeConfiguration<VTSVendorService>
    {
        public VTSVendorServiceDbMapping()
        {
            //Required Columns
            HasRequired(x => x.fk_Vendor).WithMany().HasForeignKey(x => x.VendorId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.ServiceTypeId).WillCascadeOnDelete(false);
        }
    }
}
