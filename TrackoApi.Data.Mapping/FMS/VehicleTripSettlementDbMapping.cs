using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    internal class VehicleTripSettlementDbMapping : EntityTypeConfiguration<VehicleTripSettlement>
    {
        public VehicleTripSettlementDbMapping()
        {
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DriverI).WithMany().HasForeignKey(x => x.Driver1Id).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Voucher).WithMany().HasForeignKey(x=>x.VoucherId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_SetlBalVoucher).WithMany().HasForeignKey(x => x.SetlBalVoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NetBalVoucher).WithMany().HasForeignKey(x => x.NetBalVoucherId).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_DriverII).WithMany().HasForeignKey(x => x.Driver2Id).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_Cleaner).WithMany().HasForeignKey(x => x.CleanerId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireVehicle).WithMany().HasForeignKey(x => x.HireVehicleId).WillCascadeOnDelete(false);
            Ignore(x => x.vwTripAdvances).Ignore(x=>x.vwFuelExpenses).Ignore(x=>x.vwTripExpenses).Ignore(x=>x.vwTripLogs);
            HasOptional(x=>x.fk_AdjustmentType).WithMany().HasForeignKey(x=>x.AdjustmentTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HVP).WithMany().HasForeignKey(x => x.HVPId).WillCascadeOnDelete(false);
            
        }
    }
}
