using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class HireVehicleMasterDbMapping : EntityTypeConfiguration<HireVehicle>
    {
        public HireVehicleMasterDbMapping()
        {
            HasOptional(x => x.fk_VehicleModel).WithMany().HasForeignKey(x => x.VehicleModelId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireParty).WithMany().HasForeignKey(x => x.HirePartyId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_VehicleType).WithMany().HasForeignKey(x => x.VehicleTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GPSVendor).WithMany().HasForeignKey(x => x.GPSVendorId).WillCascadeOnDelete(false);
        }
    }
}
