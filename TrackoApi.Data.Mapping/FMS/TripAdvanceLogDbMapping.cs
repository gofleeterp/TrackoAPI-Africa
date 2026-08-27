using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    internal class TripAdvanceLogDbMapping : EntityTypeConfiguration<TripAdvanceLog>
    {
        public TripAdvanceLogDbMapping()
        {
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireVehicle).WithMany().HasForeignKey(x => x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Driver).WithMany().HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_FuelType).WithMany().HasForeignKey(x => x.FuelId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CreditAccount).WithMany().HasForeignKey(x => x.CreditAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Draft).WithMany().HasForeignKey(x => x.DraftId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Settlement).WithMany(x => x.TripAdvances).HasForeignKey(x => x.SettlementId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Triplog).WithMany(x=>x.TripAdvances).HasForeignKey(x => x.TripLogId).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
           HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x=>x.VoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ActualVCH).WithMany().HasForeignKey(x => x.ActualVCHId).WillCascadeOnDelete(false);
            
            HasMany(x=>x.FuelExpanses).WithOptional(x=>x.fk_TripAdvanceLog).HasForeignKey(x=>x.TripAdvanceLogId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_PaidIn).WithMany().HasForeignKey(x => x.PaidInId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SettledRefAdvance).WithMany(x => x.SettledAdvances).HasForeignKey(x => x.SettledRefId).WillCascadeOnDelete(false);
            Ignore(x => x.DataView);
            Property(x => x.RoundUp).IsOptional();
        }
    }
}
