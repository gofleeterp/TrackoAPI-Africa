using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleMovementLog")]
    public class VehicleMovementLog : AuditableEntity,IValidatableObject/*,IAprovalEntity*/
    {
        public VehicleMovementLog()
        {
            TripAdvances = new List<TripAdvanceLog>();
            Challans = new List<ChallanMaster>();
            PODDetails = new List<CNExtraInfo>();
            TripExpenses = new List<TripExpenseLog>();
            TSLs = new List<vwTSL>();
        }
        public virtual List<vwTSL> TSLs { get; set; }
        [Column("OfficeId"), Required, ForeignKey("fk_Office")]
        public long? OfficeId { get; set; }
        public virtual OfficeMaster fk_Office { get; set; }
        [Column("SettlementId"),ForeignKey("fk_TripSettlement")]
        public long? SettlementId { get; set; }
        public virtual VehicleTripSettlement fk_TripSettlement { get; set; }
        [Column("DraftId")]
        public long? DraftId { get; set; }
        public virtual VehicleTripSettlement fk_Draft { get; set; }
        [Column("VehicleId"), ForeignKey("fk_Vehicle")]
        public long? VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }
        [Column("HireVehicleId"), ForeignKey("fk_HireVehicle")]
        public long? HireVehicleId { get; set; }
        public virtual HireVehicle fk_HireVehicle { get; set; }

        [Column("TriplogNo"), StationaryCheck, Required,Index("XI_VehicleMovementLog_TriplogNo",IsUnique = true),MaxLength(100)]
        public string TriplogNo { get; set; }
        public DateTime TripStartDate { get; set; }
        [Column("SPD")]
        public DateTime? ScheduledPlacementDate { get; set; }
        [Column("LoadingReachDate")/*,MaxFutureDate(0)*/]
        public DateTime? LoadingReachDate { get; set; }
        [Column("SDD")]
        public DateTime? ScheduledDepartureDate { get; set; }
        [Column("LoadingDate")/*, MaxFutureDate(0)*/]
        public DateTime? LoadingDate { get; set; }
        

        [Column("UnloadingReachDate")/*, MaxFutureDate(0)*/]
        public DateTime? UnloadingReachDate { get; set; }

        [Column("UnLoadingDate")/*, MaxFutureDate(0)*/]
        public DateTime? UnloadingDate { get; set; }

        [Column("RouteId"), ForeignKey("fk_Route")]
        public long? RouteId { get; set; }
        public virtual RouteMaster fk_Route { get; set; }

        [Column("OldRouteId")]
        public long? OldRouteId { get; set; }
        [ForeignKey("OldRouteId")]
        public virtual RouteMaster fk_OldRoute { get; set; }

        [Column("Remarks"),MaxLength(500)]
        public string Remarks { get; set; }

        [Column("ReportingOfficeId"), ForeignKey("fk_ReportingOffice")]
        public long? ReportingOfficeId { get; set; }
        public virtual OfficeMaster fk_ReportingOffice { get; set; }


        [Column("TrailorId"), ForeignKey("fk_Trailor")]
        public long? TrailorId { get; set; }
        public virtual VehicleMaster fk_Trailor { get; set; }

        [Column("IsHired")]
        public bool IsHired { get; set; } //Hired?true:false
        [Column("TripTypeId"), ForeignKey("fk_TripType")]
        public long? TripTypeId { get; set; } //TripLog//HireSlip//JobSheet        
        /// <summary>
        /// Gets or sets the type of the FK_ trip.
        /// Constant TypeId 76
        /// </summary>
        /// <value>The type of the FK_ trip.</value>
        public virtual ConstantValue fk_TripType { get; set; }

        [Column("TripNatureId"), ForeignKey("fk_TripNature")]
        public long? TripNatureId { get; set; } //Loaded//Empty//ORM       
        /// <summary>
        /// Gets or sets the FK_ trip nature.
        /// Constant TypeId 72
        /// </summary>
        /// <value>The FK_ trip nature.</value>
        public virtual ConstantValue fk_TripNature { get; set; }

        public long? LoadTypeId { get; set; }
        [ForeignKey("LoadTypeId")]
        public virtual LoadType fk_LoadType { get; set; }

        public long? VehicleTypeId { get; set; }
        [ForeignKey(nameof(VehicleTypeId))]
        public virtual GenericMaster fk_VehicleType { get; set; }

        [Column("TripModeId"), ForeignKey("fk_TripMode")]
        public long? TripModeId { get; set; } //Express/Highway/Normal
        public virtual GenericMaster fk_TripMode { get; set; }

        [Column("JobTypeId"), ForeignKey("fk_JobType")]
        public long? JobTypeId { get; set; } //General//Accidental//Capital        
        /// <summary>
        /// Constant TypeId 95
        /// </summary>
        public virtual ConstantValue fk_JobType { get; set; }

        [Column("Driver1stId"), ForeignKey("fk_DriverI")]
        public long? Driver1stId { get; set; }
        public virtual DriverMaster fk_DriverI { get; set; }

        [Column("Driver2ndId"), ForeignKey("fk_DriverII")]
        public long? Driver2ndId { get; set; }
        public virtual DriverMaster fk_DriverII { get; set; }

        [Column("CleanerId"), ForeignKey("fk_Cleaner")]
        public long? CleanerId { get; set; }
        public virtual DriverMaster fk_Cleaner { get; set; }


        [Column("PartyId"), ForeignKey("fk_Party")]
        public long? PartyId { get; set; } = null;
        public virtual Ledger fk_Party { get; set; }
        public int CNCount { get; set; } = 0;
        [Column("ConsigneeId"), ForeignKey("fk_Consignee")]
        public long? ConsigneeId { get; set; } = null;
        public virtual Ledger fk_Consignee { get; set; }

        [Column("MaterialId"), ForeignKey("fk_Material")]
        public long? MaterialId { get; set; } = null;
        public virtual MaterialMaster fk_Material { get; set; }
        [Column("MaterialDesc"),MaxLength(500)]
        public string MaterialDescription { get; set; }
        [Column("PodStatusId"), ForeignKey("fk_PodStatus")]
        public long? PodStatusId { get; set; }
        public virtual GenericMaster fk_PodStatus { get; set; }

        [Column("DeliveryStatusId"), ForeignKey("fk_DeliveryStatus")]
        public long? DeliveryStatusId { get; set; }
        public virtual GenericMaster fk_DeliveryStatus { get; set; }

        [Column("PodDepositAt"), MaxLength(500)]
        public string PodDepositAt { get; set; }

        [Column("StartKm")]
        public long StartKm { get; set; } = 0;

        [Column("EndKm")]
        public long EndKm { get; set; } = 0;

        [Column("KmRun")]
        public long KmRun { get; set; } = 0;

        [Column("KmRunAdd")]
        public long AdditionalKmRun { get; set; } = 0;

        [Column("TotalKmRun")]
        public long TotalKmRun { get; set; } = 0;

        [Column("GpsKmRun")]
        public long GpsKmRun { get; set; } = 0;

        [Column("ExpDeliveryDate")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Column("ExpTime")]
        public decimal ExpTime { get; set; } = 0;

        [Column("OctroiReachDate")]
        public DateTime? OctroiReachDate { get; set; }

        [Column("OctroiLeftDate")]
        public DateTime? OctroiLeftDate { get; set; }

        [Column("LoadingQty")]
        public decimal LoadingQty { get; set; } = 0;

        [Column("MktRate")]
        public decimal MktRate { get; set; } = 0;

        [Column("MarketFreight")]
        public decimal MarketFreight { get; set; } = 0;

        [Column("CNFreight")]
        public decimal CNFreight { get; set; } = 0;

        [Column("BdgtFuelExpense")]
        public decimal BdgtFuelExpense { get; set; } = 0;

        [Column("BdgtTripExpense")]
        public decimal BdgtTripExpense { get; set; } = 0;
        [Column("ConsumedFuelQty")]
        public decimal ConsumedFuelQty { get; set; }
        [Column("ConsumedFuelAmt")]
        public decimal ConsumedFuelAmt { get; set; }
        [Column("BdgtAdvance")]
        public decimal BdgtAdvance { get; set; } = 0;
        [Column("BdgtUreaQty")]
        public decimal BdgtUreaQty { get; set; } = 0;

        [Column("BdgtFuelQty")]
        public decimal BdgtFuelQty { get; set; } = 0;

        [Column("AddFuelQty")]
        public decimal AdditionalFuelQty { get; set; } = 0;

        [Column("ReferFuelQty")]
        public decimal ReferFuelQty { get; set; } = 0;

        [Column("UOLessI")]
        public decimal UOLessI { get; set; } = 0;

        [Column("UOLessII")]
        public decimal UOLessII { get; set; } = 0;

        [Column("HSTDSRate")]
        public decimal HSTDSRate { get; set; } = 0;

        [Column("HSTDSAmount")]
        public decimal HSTDSAmount { get; set; } = 0;

        [Column("VoucherDate")]
        public DateTime? VoucherDate { get; set; }

        [Column("FormId"), Required, MaxLength(40)]
        public string FormId { get; set; }

        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        public virtual List<TripAdvanceLog> TripAdvances { get; set; }
        public virtual List<HSAdvance> HSAdvances { get; set; } 
        public virtual List<TripExpenseLog> TripExpenses { get; set; } 
        public virtual List<ChallanMaster> Challans { get; set; }
        public virtual List<CNExtraInfo> PODDetails { get; set; }

        public decimal ShortFuelAmt { get; set; }

        public decimal ShortFuelQty { get; set; }
        public long? RepairSupervisorId { get; set; }
        [ForeignKey("RepairSupervisorId")]
        public virtual GenericMaster fk_RepairSupervisor { get; set; }
        public long? TyreSupervisorId { get; set; }
        [ForeignKey("TyreSupervisorId")]
        public virtual GenericMaster fk_TyreSupervisor { get; set; }
        [MaxLength(500)]
        public string OtherRemark { get; set; }
        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public virtual VehicleMovementLog fk_NextLog { get; set; }
        public long? PreviousLogId { get; set; }
        [ForeignKey("PreviousLogId")]
        public virtual VehicleMovementLog fk_PreviousLog { get; set; }

        public long? ParentTLId { get; set; }
        [ForeignKey("ParentTLId")]
        public virtual VehicleMovementLog fk_ParentTL { get; set; }

        public bool IsGPSAttached { get; set; }
        /// <summary>
        /// Gets or sets the loaded by identifier.
        /// Constant Id 1456
        /// </summary>
        /// <value>The loaded by identifier.</value>
        
        public long? LoadedById { get; set; }
        [ForeignKey("LoadedById")]
        public virtual GenericMaster fk_LoadedBy { get; set; }
        [MaxLength(300)]
        public string LoadedBy { get; set; }
        /// <summary>
        /// Gets or sets the unloaded by identifier.
        /// Constant Id 1456
        /// </summary>
        /// <value>The unloaded by identifier.</value>
        public long? UnloadedById { get; set; }
        [ForeignKey("UnloadedById")]
        public virtual GenericMaster fk_UnloadedBy { get; set; }

        [MaxLength(300)]
        public string UnloadedBy { get; set; }
        public virtual List<CnChallan> ChallanCNs { get; set; }
        #region Lorry Hire Section
        public long? HireOwnerId { get; set; }
        [ForeignKey("HireOwnerId")]
        public virtual Ledger fk_HireOwner { get; set; }
        public long? HVPId { get; set; }
        [ForeignKey("HVPId")]
        public virtual Ledger fk_HVP { get; set; }
        public long? BrokerId { get; set; }
        [ForeignKey("BrokerId")]
        public virtual Ledger fk_Broker { get; set; }
        [MaxLength(300)]
        public string BrokerName { get; set; }
        [Column("HireChargesAcId")]
        public long? HVPayableId { get; set; }
        [ForeignKey("HVPayableId")]
        public virtual Ledger fk_HVPayable { get; set; }
        [Column("PanStatusId")]
        public long? PANStatusId { get; set; }
        [ForeignKey("PanStatusId")]
        public virtual ConstantValue fk_PANStatus { get; set; }
        [MaxLength(10)]
        public string PANNo { get; set; }
        public decimal HSChg { get; set; } = 0;
        public decimal MiscChg { get; set; } = 0;
        public decimal TCChg { get; set; } = 0;
        public int HSDetentionDays { get; set; } = 0;
        public decimal HSDetentionRate { get; set; } = 0;
        public decimal HSDetention { get; set; } = 0;

        public decimal HSAddChg4 { get; set; } = 0;
        public decimal HSUnloadCharges { get; set; } = 0;
        public decimal HSPenalty { get; set; } = 0;
        public decimal HSClaims { get; set; } = 0;
        public decimal HSAddChg1 { get; set; } = 0;
        public decimal HSAddChg2 { get; set; } = 0;
        public decimal HSAddChg3 { get; set; } = 0;
        public decimal HSLessChg1 { get; set; } = 0;
        public decimal HSLessChg2 { get; set; } = 0;
        public decimal HSLessChg3 { get; set; } = 0;
        public decimal HSLessChg4 { get; set; } = 0;
        public decimal TotalHSAmount { get; set; } = 0;
        public decimal LoadedWeight { get; set; } = 0;        
        [MaxLength(150)]
        public string DriverName { get; set; }
        [MaxLength(15)]
        public string DriverPhone { get; set; }
        public long? TransportModeId { get; set; }
        [ForeignKey("TransportModeId")]
        public virtual ConstantValue fk_TransportMode { get; set; }

        public long? FromPlaceId { get; set; }
        [ForeignKey("FromPlaceId")]
        public virtual CityMaster fk_FromPlace { get; set; }

        public long? ToPlaceId { get; set; }
        [ForeignKey("ToPlaceId")]
        public virtual CityMaster fk_ToPlace { get; set; }

        public DateTime? TerminationDate { get; set; }
        /// <summary>
        /// Terminated Trip Id for which this trip has been continued
        /// </summary>
        public long? TerminatedTripId { get; set; }
        [ForeignKey("TerminatedTripId")]
        public virtual VehicleMovementLog fk_TerminatedTrip { get; set; }

        [Column("RouteDesc"),MaxLength(1000)]
        public string RouteDesc { get; set; }


        [Column("HMNo"), MaxLength(100)]
        public string HMNo { get; set; }


        //Use for manual entry of route in case local movement of vehicle in single day

        #endregion
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            
            if (string.IsNullOrWhiteSpace(TriplogNo))
            {
                yield return new ValidationResult($"Document No is Required",new []{ "TriplogNo" });
            }

            //if(VehicleId==null && HireVehicleId==null)
            //{
            //    yield return new ValidationResult("Either Own or Hired Vehicle is Required for creating Trip. ", new[] { "VehicleId", "HireVehicleId" });
            //}
            //if (LoadingReachDate != null && TripStartDate > LoadingReachDate)
            //{
            //    yield return new ValidationResult($"Trip No :{this.TriplogNo} Loading ReachDate has to be greater than Trip Start Date", new[] { "LoadingReachDate" });
            //}
            //if (LoadingDate != null && LoadingReachDate.GetValueOrDefault(TripStartDate) > LoadingDate)
            //{
            //    yield return new ValidationResult($"Trip No :{this.TriplogNo} Loading Date has to be greater than Loading ReachDate", new[] { "LoadingDate" });
            //}
            //if (UnloadingReachDate != null && LoadingDate.GetValueOrDefault(LoadingReachDate.GetValueOrDefault(TripStartDate)) > UnloadingReachDate)
            //{
            //    yield return new ValidationResult($"Trip No :{this.TriplogNo} Unloading Reach Date has to be greater than Loading Date", new[] { "UnloadingReachDate" });
            //}
            //if (UnloadingDate != null && UnloadingReachDate.GetValueOrDefault(LoadingDate.GetValueOrDefault(LoadingReachDate.GetValueOrDefault(TripStartDate))) > UnloadingDate)
            //{
            //    yield return new ValidationResult($"Trip No :{this.TriplogNo} UnloadingDate {UnloadingDate:dd-MMM-yyyy HH:mm:ss tt} has to be greater than Unloading Reach Date  {UnloadingReachDate.GetValueOrDefault(LoadingDate.GetValueOrDefault(LoadingReachDate.GetValueOrDefault(TripStartDate))):dd-MMM-yyyy HH:mm:ss tt} AND Loading Date", new[] { "UnloadingDate" });
            //}
        }
        [MaxLength(255)]
        public string BatchId { get; set; }

        public virtual List<VTSStatusLog> VTSLogs { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        public long? VoucherId { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [MaxLength(50)]
        public string DriverPhoneII { get; set; }
        [Column("ExtraProperties")]
        public string ExtraProperties { get; set; }
        public long? VDRId { get; set; }
        [ForeignKey("VDRId")]
        public virtual VoucherDetailReference fk_VDR { get; set; }

        public long? BillId { get; set; }
        [ForeignKey("BillId")]
        public virtual CNBill fk_Bill { get; set; }

        public long? HSTDSVoucherId { get; set; }
        [ForeignKey("HSTDSVoucherId")]
        public virtual Voucher fk_TDSVoucher { get; set; }

        [Column("Date1")]
        public DateTime? Date1 { get; set; }

        public long? Location1Id { get; set; }
        [ForeignKey("Location1Id")]
        public virtual CityMaster fk_Location1Id { get; set; }

        [Column("Date2")]
        public DateTime? Date2 { get; set; }

        public long? Location2Id { get; set; }
        [ForeignKey("Location2Id")]
        public virtual CityMaster fk_Location2Id { get; set; }

        [MaxLength(150)]
        public string EWayBillTL { get; set; }
        public DateTime? eWayBillValidity { get; set; }
        [Column("VehicleAvg")]
        public decimal VehicleAvg { get; set; } = 0;
        [Column("EmptyAvg")]
        public decimal EmptyAvg { get; set; } = 0;

        public List<VehicleMovementLogPickupDrop> WayPoints { get; set; }=new List<VehicleMovementLogPickupDrop>();
        public bool IsApproved { get; set; } = false;
        //public DateTime? APRLDateTime { get; set; }
        //public string APRLRemark { get; set; }
        //public long? APRLSID { get; set; }
        //public long? APRLUserId { get; set; }
        //public bool IsAutoAPRL { get; set; } = false;
        public long? VTSStatuslogId { get; set; }
        [ForeignKey("VTSStatuslogId")]
        public virtual VTSStatusLog fk_VTSStatusLog { get; set; }

        public bool CreateWayPointOnServer { get; set; }
        public bool BookBudgetingOnServer { get; set; }
        public bool RefreshBudgetingOnServer { get; set; } 
        [ForeignKey("ScheduleConfigurationId")]
        public virtual TripScheduleConfiguration fk_ScheduleConfiguration { get; set; }
        public long? ScheduleConfigurationId { get; set; }
        [MaxLength(110)]
        public string PartyRefNo { get; set; }
        public string CnChallanJson { get; set; }
        public long? Ledger1Id { get; set; }
        [ForeignKey("Ledger1Id")]
        public virtual Ledger Ledger1 { get; set; }
      
        public long? MaterialInvoiceId { get; set; }
        [ForeignKey("fk_MaterialInvoice")]
        public virtual SpareLogExtraInfo fk_MaterialInvoice { get; set; }

        public long? MaterialSaleId { get; set; }
        [ForeignKey("fk_MaterialSale")]
        public virtual SpareLogExtraInfo fk_MaterialSale { get; set; }

        [Column("Loading_ABW")]
        public decimal Loading_ABW { get; set; } = 0;
        
        [Column("Loading_20C")]
        public decimal Loading_20C { get; set; } = 0;
        

        [Column("OffLd_ABW")]
        public decimal OffLd_ABW { get; set; } = 0;

        [Column("OffLd_20C")]
        public decimal OffLd_20C { get; set; } = 0;

        [Column("SHRT_AllowedP")]
        public decimal SHRT_AllowedP { get; set; } = 0;

        [Column("SHRT_AllowedWt")]
        public decimal SHRT_AllowedWt { get; set; } = 0;

        [Column("SHRT_ReImburse")]
        public decimal SHRT_ReImburse { get; set; } = 0;

        
        [Column("SHRT_Wt")]
        public decimal SHRT_Wt { get; set; } = 0;

        [Column("SHRT_Rate")]
        public decimal SHRT_Rate { get; set; } = 0;

        [Column("SHRT_Amount")]
        public decimal SHRT_Amount { get; set; } = 0;

        [Column("SHRT_VoucherDate")/*, SHRT_VoucherDate*/]
        public DateTime? SHRT_VoucherDate { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(ExtraProperties)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties)): _dt;
            get
            {
                try
                {
                    if (ExtraProperties == "{}") ExtraProperties = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties ?? (ExtraProperties = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }
                
            }
            set
            {
                _dt = value;
                ExtraProperties = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }


        }
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((ExtraProperties ?? "{}") == "{}") ExtraProperties = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((ExtraProperties ?? (ExtraProperties = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                ExtraProperties = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                ExtraProperties = "[]";
            }
        }

    }
}