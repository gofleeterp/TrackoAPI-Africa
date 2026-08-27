using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    internal class BatteryBrandDbMapping : EntityTypeConfiguration<BatteryBrand>
    {
        public BatteryBrandDbMapping()
        {
            HasRequired(x => x.fk_Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId).WillCascadeOnDelete(false);
        }
    }
}