// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VehicleTripcs" company="">
//   
// </copyright>
// <summary>
//   Defines the VehicleTripSettlement type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using TrackoApi.Models.AMS;
using TrackoApi.Models.Validations;
using TrackoAPI.vw.ts;

namespace TrackoApi.Models.FMS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Base;
    using Global;

    using TrackoApi.Core.Helpers;
    using TrackoApi.Models.BMS;

    /// <summary>
    /// The vehicle trip 
    /// </summary>
    [Table("tTripSettlement")]
    public class VehicleTripSettlement : AuditableEntity,IValidatableObject
    {
        
        #region Other Properties
        /// <summary>
        /// Office
        /// </summary>
        [Column("OfficeId"),ForeignKey("fk_Office"),Required]
        public long OfficeId { get; set; }

        /// <summary>
        /// Gets or sets the fk_ office.
        /// </summary>
        public virtual OfficeMaster fk_Office { get; set; }
        /// <summary>
        /// Begin Date
        /// </summary>
        [Column("StartDate"), Required]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Trip No
        /// </summary>
        [Column("TripSheetNo"), StationaryCheck, Required,Index("IX_tTripSettlement_TripSheetNo",IsUnique = true),MaxLength(100)]
        public string TripSheetNo { get; set; }
        /// <summary>
        /// End Date
        /// </summary>
        [Column("EndDate")]
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Settle Date
        /// </summary>
        [Column("SettleDate")]
        public DateTime? SettleDate { get; set; }
        /// <summary>
        /// Fleet No
        /// </summary>
        [Column("VehicleId"), ForeignKey("fk_Vehicle")]
        public long? VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        /// <summary>
        /// Fleet No
        /// </summary>
        [Column("HireVehicleId"), ForeignKey("fk_HireVehicle")]
        public long? HireVehicleId { get; set; }
        public virtual HireVehicle fk_HireVehicle { get; set; }
        /// <summary>
        /// I-Driver
        /// </summary>
        [Column("Driver1Id"), ForeignKey("fk_DriverI")]
        public long? Driver1Id { get; set; }
        public virtual DriverMaster fk_DriverI { get; set; }
        /// <summary>
        /// TripRoute
        /// </summary>
        [Column("TripRoute")]
        public string TripRoute { get; set; }

        /// <summary>
        /// Remarks
        /// </summary>
        [Column("Remarks")]
        [MaxLength(300)]
        public string Remarks { get; set; }
        #endregion
        #region KM Calculation

        /// <summary>
        /// StartKm
        /// </summary>
        [Column("StartKm")]
        public long StartKm { get; set; }
        /// <summary>
        /// End Km
        /// </summary>
        [Column("EndKm")]
        public long EndKm { get; set; }
        /// <summary>
        /// Run Km
        /// </summary>
        [Column("RunKm")]
        public long RunKm { get; set; }
        /// <summary>
        /// Additional Run Km
        /// </summary>
        [Column("AddRunKm")]
        public long AddRunKm { get; set; }
        /// <summary>
        /// Total Run Km
        /// </summary>
        [Column("TotalKmRun")]
        public long TotalKmRun { get; set; }
        /// <summary>
        /// Total Refer Hour
        /// </summary>
        [Column("TotalReferHour")]
        public long TotalReferHour { get; set; }
        #endregion
        #region Diesel Accounting

        /// <summary>
        /// Fuel Quantity
        /// </summary>
        [Column("BdgtFuelQuantity")]
        public decimal FuelQuantity { get; set; } = 0;
        /// <summary>
        /// Refer Quantity
        /// </summary>
        [Column("BdgtReferQuantity")]
        public decimal ReferQuantity { get; set; } = 0;

        /// <summary>
        /// Extra Fuel Quantity
        /// </summary>
        [Column("BdgtExtraFuelQty")]
        public decimal ExtraFuelQty { get; set; } = 0;

        /// <summary>
        /// Budgeted Qty Total i.e FuelQuantity+ReferQuantity+ExtraFuelQty
        /// </summary>
        [Column("TotalBdgtFuelQty")]
        public decimal TotalBudgetedFuelQty { get; set; }

        /// <summary>
        /// Actual Qty
        /// </summary>
        [Column("ActualQty")]
        public decimal ActualQty { get; set; }

        [Column("ShortageQty")]
        public decimal ShortageQty { get; set; }
        /// <summary>
        /// Difference
        /// </summary>
        [Column("FuelQtyDifference")]
        public decimal FuelQtyDifference { get; set; }//FuelBalanceQty
        [Column("FuelAmtDiff")]
        public decimal FuelAmountDifference { get; set; }//FuelBalanceAmount=New

        public decimal NetBalanceFuelRate { get; set; } = 0;
        [Column("BookingFreight")]
        public decimal BookingFreight { get; set; } = 0;

        public long PreviousKM { get; set; } = 0;
        public long PrepaidKm { get; set; } = 0;
        public decimal PreviousExp { get; set; } = 0;
        public decimal PrepaidExp { get; set; } = 0;
        public void Compute()
        {
            if (TripAdvances != null)
            {
                ActualQty = TripAdvances.Where(x => x.AdvanceTypeId == 2 && x.FuelQty > 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.FuelQty);
            }
            if (TripExpenses != null)
            {
                ActualQty += TripExpenses.Where(x => x.FuelQty > 0 && x.TripAdvanceLogId > 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.FuelQty);
            }
            TotalBudgetedFuelQty= FuelQuantity + ReferQuantity + ExtraFuelQty;
            FuelQtyDifference = TotalBudgetedFuelQty - (ActualQty + ShortageQty);
            if (TripExpenses != null)
            {
                TripExpenseAmt = (TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) == 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.SettledAmount));
                FuelExpenseAmt = (TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.SettledAmount));
                ShortageQty = (TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.ShortFuelQty));
                ShortageFuelAmt = (TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0 && x.ObjectState != ObjectState.Deleted).Sum(x => x.ShortFuelAmt));

            }
            if (TripAdvances != null)
            {
                TripAdvanceAmt = (TripAdvances.Where(x => x.AdvanceTypeId == 1 || x.AdvanceTypeId == 2).Sum(x => x.CashAmount + x.FuelAmount));
            }
            if (TripAdvances != null)
            {
                PenaltyAmount = TripAdvances.Where(x => x.AdvanceTypeId == 16).Sum(x => x.CashAmount + x.FuelAmount);
            }
            if (TripLogs.Any()&& TripExpenses!=null&&TripExpenses.Any(x=>x.TripAdvanceLogId>0))
            {
                BookingFreight = TripLogs.Sum(x => x.CNFreight);
                foreach (var log in TripLogs)
                {
                    var f = TripExpenses.Where(x => x.TripAdvanceLogId > 0 && x.TripLogId == log.Id).Select(x=>new {x.FuelQty,x.SettledAmount,x.ShortFuelQty,x.ShortFuelAmt}).ToList();
                    log.ConsumedFuelAmt = f.Sum(x => x.SettledAmount);
                    log.ConsumedFuelQty = f.Sum(x => x.FuelQty);
                    log.ShortFuelQty = f.Sum(x => x.ShortFuelQty);
                    log.ShortFuelAmt = f.Sum(x => x.ShortFuelAmt);

                    log.ObjectState=ObjectState.Modified;
                }
            }
            SettledAmount=(TripAdvanceAmt+ShortageFuelAmt+ PenaltyAmount + CashPaid) -(TripExpenseAmt+ FuelExpenseAmt+CashDeposited);

        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var settlementviewids = new long?[] { 1574, 1009 };
            if (settlementviewids.Contains(ViewId) && EndDate == null)
            {
                yield return new ValidationResult("Settlement EndDate is required", new[] { "EndDate" });
            }
        }
        #endregion
        #region Settlement Accounting
        [Column("PenaltyAmount")]
        public decimal PenaltyAmount { get; set; } = 0;

        [Column("CashDeposited")]
        public decimal CashDeposited { get; set; } = 0;
        public long? CashDepositAccId { get; set; }
        [ForeignKey(nameof(CashDepositAccId))]
        public virtual Ledger fk_CashDepositAcc { get; set; }
        [Column("CashPaid")]
        public decimal CashPaid { get; set; } = 0;
        public long? CashPaidAccId { get; set; }
        [ForeignKey(nameof(CashPaidAccId))]
        public virtual Ledger fk_CashPaidAcc { get; set; }
        //[ForeignKey("CashPaidAdvVchId")]
        //public virtual Voucher fk_CashPaidAdvVoucher { get; set; }
        //[Column("CashPaidAdvVchId")]
        //public long? CashPaidAdvVchId { get; set; }
        [ForeignKey("CashPaidAdvId")]
        public virtual TripAdvanceLog fk_CashPaidAdv { get; set; }
        [Column("CashPaidAdvId")]
        public long? CashPaidAdvId { get; set; }
        /// <summary>
        /// Trip Expense
        /// </summary>
        [Column("TripExpense")]
        public decimal TripExpenseAmt { get; set; } = 0;
        /// <summary>
        /// Fuel Expense i.e. Where Advance Type is Driver Diesel Expanses
        /// </summary>
        [Column("FuelExpense")]
        public decimal FuelExpenseAmt { get; set; } = 0;
        /// <summary>
        /// Trip Advance i.e. where Advance Type is Driver Diesel Advance and Driver Cash Advance
        /// </summary>
        [Column("TripAdvance")]
        public decimal TripAdvanceAmt { get; set; } = 0;
        [Column("FuelAdvance")]
        public decimal FuelAdvanceAmt { get; set; } = 0;
        /// <summary>
        /// Shortage Amount for Fuel Expanses
        /// </summary>
        [Column("Shortage")]
        public decimal ShortageFuelAmt { get; set; } = 0;

        /// <summary>
        /// Gets or Sets Driver Payment made through Settlement
        /// </summary>
        [Column("DriverPayment")]
        public decimal DriverPayment { get; set; } = 0;

        /// <summary>
        /// Gets or sets the settled amount.
        /// </summary>
        /// <value>The settled amount.</value>
        [Column("SettledAmount")]
        public decimal SettledAmount { get; set; } = 0;
        [Column("SettledAccountId"),ForeignKey("fk_SettlementAccount")]
        public long? SettlementAccountId { get; set; }
        public virtual Ledger fk_SettlementAccount { get; set; }
        [Column("VoucherId"),ForeignKey("fk_Voucher")]
        public long? VoucherId { get; set; }

        public virtual Voucher fk_Voucher { get; set; }
        [Column("SetlBalVoucherId"), ForeignKey("fk_SetlBalVoucher")]

        public long? SetlBalVoucherId { get; set; }

        public virtual Voucher fk_SetlBalVoucher { get; set; }
        [Column("SetlBalFuelVoucherId"), ForeignKey("fk_SetlBalFuelVoucher")]
        public long? SetlBalFuelVoucherId { get; set; }

        public virtual Voucher fk_SetlBalFuelVoucher { get; set; }

        public virtual Voucher fk_NetBalVoucher { get; set; }
        [Column("NetBalVoucherId"), ForeignKey("fk_NetBalVoucher")]
        public long? NetBalVoucherId { get; set; }
        public bool NetBalancePending { get; set; }
        #endregion
        #region Navigation Properties
        /// <summary>
        /// Gets or sets the trip expenses.
        /// </summary>
        public virtual List<TripExpenseLog> TripExpenses { get; set; }

        /// <summary>
        /// Gets or sets the trip advances.
        /// </summary>
        public virtual List<TripAdvanceLog> TripAdvances { get; set; }

        /// <summary>
        /// Gets or sets the trip logs.
        /// </summary>
        public virtual List<VehicleMovementLog> TripLogs { get; set; }
        /// <summary>
        /// Gets or sets the Trip expenses for Insert/Update/Delete.
        /// </summary>
        /// <value>The vw trip expenses.</value>
        public virtual List<TrackoAPI.vw.ts.Expense> vwTripExpenses { get; set; }
        /// <summary>
        /// Gets or sets the Trip advances for Insert/Update/Delete..
        /// </summary>
        /// <value>The vw trip advances.</value>
        public virtual List<TrackoAPI.vw.ts.Advance> vwTripAdvances { get; set; }
        /// <summary>
        /// Gets or sets the Trip logs for Insert/Update/Delete.
        /// </summary>
        /// <value>The vw trip logs.</value>
        public virtual List<TrackoAPI.vw.ts.TripLog> vwTripLogs { get; set; }
        public virtual List<TrackoAPI.vw.ts.FuelExpense> vwFuelExpenses { get; set; }
        #endregion

        public long? ViewId { get; set; }
        [MaxLength(150)]
        public string BatchId { get; set; }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        public VehicleTripSettlement()
        {
            TripExpenses=new List<TripExpenseLog>();
            TripAdvances=new List<TripAdvanceLog>();
            TripLogs=new List<VehicleMovementLog>();
            vwTripExpenses=new List<Expense>();
            vwTripAdvances=new List<Advance>();
            vwTripLogs=new List<TripLog>();
            vwFuelExpenses=new List<FuelExpense>();
        }
        /// <summary>
        /// Constant Type Id
        /// </summary>
        [Column("AdjustmentTypeId"), ForeignKey("fk_AdjustmentType")]
        public long? AdjustmentTypeId { get; set; }
        public virtual ConstantValue fk_AdjustmentType { get; set; }

        [Column("HVPId"), ForeignKey("fk_HVP")]
        public long? HVPId { get; set; }
        public virtual Ledger fk_HVP { get; set; }
        public long? TDSAccountId { get; set; }
        [ForeignKey("TDSAccountId")]
        public virtual Ledger fk_TDSAccount { get; set; }
        public decimal TDSAmount { get; set; }
        public long? SettlementTypeId { get; set; }
        [ForeignKey("SettlementTypeId")]
        public virtual ConstantValue fk_SettlementType { get; set; }
    }
}
