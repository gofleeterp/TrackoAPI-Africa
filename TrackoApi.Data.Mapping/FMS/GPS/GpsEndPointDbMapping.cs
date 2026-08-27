using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.GPS;

namespace TrackoApi.Data.Mapping.FMS.GPS
{
    internal class GpsEndPointDbMapping : EntityTypeConfiguration<GpsEndPoint>
    {
        public GpsEndPointDbMapping()
        {
            HasRequired(x=>x.fk_ServiceType).WithMany().HasForeignKey(x=>x.ServiceTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vendor).WithMany().HasForeignKey(x => x.VendorId).WillCascadeOnDelete(false);
            Ignore(x => x.Headers);
            Property(x => x._Headers).HasColumnName("Headers");
        }
    }
}