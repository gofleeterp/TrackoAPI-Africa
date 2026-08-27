using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleMovementLogDbMapping : EntityTypeConfiguration<VehicleMovementLog>
    {
        public VehicleMovementLogDbMapping()
        {
            Ignore(x => x.CnChallanJson);
            Ignore(x => x.TSLs);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_JobType).WithMany().HasForeignKey(x => x.JobTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Cleaner).WithMany().HasForeignKey(x => x.CleanerId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DeliveryStatus).WithMany().HasForeignKey(x => x.DeliveryStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DriverI).WithMany().HasForeignKey(x => x.Driver1stId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DriverII).WithMany().HasForeignKey(x => x.Driver2ndId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Material).WithMany().HasForeignKey(x => x.MaterialId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Party).WithMany().HasForeignKey(x => x.PartyId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PodStatus).WithMany().HasForeignKey(x => x.PodStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ReportingOffice).WithMany().HasForeignKey(x => x.ReportingOfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Route).WithMany().HasForeignKey(x => x.RouteId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Trailor).WithMany().HasForeignKey(x => x.TrailorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripMode).WithMany().HasForeignKey(x => x.TripModeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripType).WithMany().HasForeignKey(x => x.TripTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripNature).WithMany().HasForeignKey(x => x.TripNatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasMany(x=>x.Challans).WithOptional(x=>x.fk_Triplog).HasForeignKey(x=>x.TriplogId).WillCascadeOnDelete(true);
            HasMany(x=>x.TripExpenses).WithRequired(x=>x.fk_TripLog).HasForeignKey(x=>x.TripLogId).WillCascadeOnDelete(true);
            HasOptional(x=>x.fk_TripSettlement).WithMany(x=>x.TripLogs).HasForeignKey(x=>x.SettlementId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Draft).WithMany().HasForeignKey(x => x.DraftId).WillCascadeOnDelete(false);
            HasMany(x => x.TripAdvances).WithOptional(x => x.fk_Triplog).HasForeignKey(x => x.TripLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NextLog).WithMany().HasForeignKey(x => x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PreviousLog).WithMany().HasForeignKey(x => x.PreviousLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TerminatedTrip).WithMany().HasForeignKey(x => x.TerminatedTripId).WillCascadeOnDelete(false);
            //Lorryhire
            HasOptional(x => x.fk_HireOwner).WithMany().HasForeignKey(x => x.HireOwnerId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HVP).WithMany().HasForeignKey(x => x.HVPId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Broker).WithMany().HasForeignKey(x => x.BrokerId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HVPayable).WithMany().HasForeignKey(x => x.HVPayableId).WillCascadeOnDelete(false);
            HasMany(x=>x.HSAdvances).WithOptional(x=>x.fk_HireSlip).HasForeignKey(x=>x.HireSlipId).WillCascadeOnDelete(true);
            HasOptional(x=>x.fk_PANStatus).WithMany().HasForeignKey(x=>x.PANStatusId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_LoadedBy).WithMany().HasForeignKey(x=>x.LoadedById).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_UnloadedBy).WithMany().HasForeignKey(x => x.UnloadedById).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_HireVehicle).WithMany().HasForeignKey(x=>x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TransportMode).WithMany().HasForeignKey(x => x.TransportModeId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_FromPlace).WithMany().HasForeignKey(x => x.FromPlaceId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ToPlace).WithMany().HasForeignKey(x => x.ToPlaceId).WillCascadeOnDelete(false);
            HasMany(x=>x.VTSLogs).WithOptional(x=>x.fk_Triplog).HasForeignKey(x=>x.TriplogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TDSVoucher).WithMany().HasForeignKey(x => x.HSTDSVoucherId).WillCascadeOnDelete(false);
            /*added by sanjay 2020-01-20*/
            HasOptional(x => x.fk_VTSStatusLog).WithMany().HasForeignKey(x => x.VTSStatuslogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_MaterialInvoice).WithMany().HasForeignKey(x => x.MaterialInvoiceId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_MaterialSale).WithMany().HasForeignKey(x => x.MaterialSaleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CurType).WithMany().HasForeignKey(x => x.CurTypeId).WillCascadeOnDelete(false);
            Ignore(x => x.CreateWayPointOnServer);
            Ignore(x => x.BookBudgetingOnServer);
            Ignore(x => x.RefreshBudgetingOnServer);
            Ignore(x => x.Data);
        }
    }
}
