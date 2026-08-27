using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class ChallanMasterDbMapping:EntityTypeConfiguration<ChallanMaster>
    {
        public ChallanMasterDbMapping()
        {
            HasMany(x=>x.CNChallans).WithOptional(x=>x.fk_Challan).HasForeignKey(x=>x.ChallanId).WillCascadeOnDelete(true);
            //HasOptional(x=>x.fk_ChallanMode).WithMany().HasForeignKey(x=>x.ChallanModeId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Driver).WithMany().HasForeignKey(x=>x.DriverId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_ChallanType).WithMany().HasForeignKey(x=>x.ChallanTypeId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Vehicle).WithMany().HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireVehicle).WithMany().HasForeignKey(x => x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Route).WithMany().HasForeignKey(x => x.RouteId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Triplog).WithMany(x=>x.Challans).HasForeignKey(x=>x.TriplogId).WillCascadeOnDelete(true);

            HasOptional(x => x.fk_Consignee).WithMany().HasForeignKey(x => x.ConsigneeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Consignor).WithMany().HasForeignKey(x => x.ConsignorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BillingParty).WithMany().HasForeignKey(x => x.BillingPartyId).WillCascadeOnDelete(false);
            Ignore(x => x.CnChallanJson);

            Ignore(x => x.ChallanCNView);
        }
    }
}
