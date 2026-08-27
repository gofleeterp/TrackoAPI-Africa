using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleMasterDbMapping:EntityTypeConfiguration<VehicleMaster>
    {
        public VehicleMasterDbMapping()
        {
            HasOptional(x => x.fk_CurType).WithMany().HasForeignKey(x => x.CurTypeId).WillCascadeOnDelete(false);

            HasRequired(x => x.fk_VehicleOwner).WithMany().HasForeignKey(x => x.OwnerPartyId).WillCascadeOnDelete(false);            
            HasRequired(x => x.fk_VehicleModel).WithMany().HasForeignKey(x => x.VehicleModelId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_VehicleType).WithMany().HasForeignKey(x => x.VehicleTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RefI).WithMany().HasForeignKey(x => x.Ref1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_RefII).WithMany().HasForeignKey(x => x.Ref2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GPSVendor).WithMany().HasForeignKey(x => x.GPSVendorId).WillCascadeOnDelete(false);

            HasKey(x => x.Id);
            HasRequired(x => x.fk_VehicleLedger).WithOptional().WillCascadeOnDelete(false);
            Ignore(x => x.AccountDetail);
            Ignore(x => x.Aliases);
            //HasMany(x=>x.VTSLogs).WithOptional(x=>x.fk_Vehicle).HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Financier).WithMany().HasForeignKey(x => x.FinancierId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Dealer).WithMany().HasForeignKey(x => x.DealerId).WillCascadeOnDelete(false);
            HasMany(x=>x.Cards).WithRequired(x=>x.fk_Vehicle).HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
            Property(x => x.GeographicPoint).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed).IsOptional();
            

        }
    }
}
