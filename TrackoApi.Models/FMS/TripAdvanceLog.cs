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
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tTripAdvanceLog")]
    public class TripAdvanceLog:AuditableEntity,IValidatableObject,IAprovalEntity
    {
        public TripAdvanceLog()
        {
            ObjectState=ObjectState.Unchanged;
            FuelExpanses = new List<TripExpenseLog>();
        }
        [Column("VoucherNo"), StationaryCheck, Required, MaxLength(100), MinLength(3),Index("IX_TripAdvanceLog_VoucherNo",IsUnique = false)]
        public string VoucherNo { get; set; }
        [Column("VehicleId"),ForeignKey("fk_Vehicle")]
        public long? VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }
       
        [DataType(DataType.Date)]
        public DateTime AdvanceDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        [Column("OfficeId"), ForeignKey("fk_Office")]
        public long OfficeId { get; set; }

        public virtual OfficeMaster fk_Office { get; set; }

        [Column("CreditAccountId"), ForeignKey("fk_CreditAccount"), Index("IX_TripAdvanceLog_ReferenceNo", IsUnique = true, Order = 1)]
        public long? CreditAccountId { get; set; }
        public virtual Ledger fk_CreditAccount { get; set; }

        [Column("DebitAccountId"), ForeignKey("fk_DebitAccount")]
        public long? DebitAccountId { get; set; }

        public virtual Ledger fk_DebitAccount { get; set; }
        
        public long? IGSTAccountId { get; set; }
        [ForeignKey(nameof(IGSTAccountId))]
        public virtual Ledger fk_IGSTAccount { get; set; }

        public long? CGSTAccountId { get; set; }
        [ForeignKey(nameof(CGSTAccountId))]
        public virtual Ledger fk_CGSTAccount { get; set; }
        
        public long? SGSTAccountId { get; set; }
        [ForeignKey(nameof(SGSTAccountId))]
        public virtual Ledger fk_SGSTAccount { get; set; }
        public long? RoundUpAccountId { get; set; }
        [ForeignKey(nameof(RoundUpAccountId))]
        public virtual Ledger fk_RoundUpAccount { get; set; }
        public long? HSNCodeId { get; set; }
        [ForeignKey("HSNCodeId")]
        public virtual TaxServiceType fk_HSNCode { get; set; }

        [Column("CashAmount")]
        public decimal CashAmount { get; set; } = 0;

        [Column("LoanAdjusted")]
        public decimal LoanAdjusted { get; set; } = 0;

        [Column("PaidAmount")]
        public decimal PaidAmount { get; set; } = 0;

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;

        public decimal IGSTAmt { get; set; } = 0;

        public decimal CGSTAmt { get; set; } = 0;

        public decimal SGSTAmt { get; set; } = 0;

        public decimal IGSTRate { get; set; } = 0;

        public decimal CGSTRate { get; set; } = 0;

        public decimal SGSTRate { get; set; } = 0;
        public decimal RoundUp { get; set; } = 0;
        
        [Column("BasicAmt")]
        public decimal BasicAmt { get; set; } = 0;
       

        [Column("FuelId"), ForeignKey("fk_FuelType")]
        public long? FuelId { get; set; }
        public virtual GenericMaster fk_FuelType { get; set; }

        [Column("FuelQty")]
        public decimal FuelQty { get; set; } = 0;

        [Column("FuelRate")]
        public decimal FuelRate { get; set; } = 0;

        [Column("FuelAmount")]
        public decimal FuelAmount { get; set; } = 0;

        [Column("SettlementId"), ForeignKey("fk_Settlement")]
        public long? SettlementId { get; set; }
        public virtual VehicleTripSettlement fk_Settlement { get; set; }
        [Column("DraftId")]
        public long? DraftId { get; set; }
        public virtual VehicleTripSettlement fk_Draft { get; set; }

        [Column("DriverId"), ForeignKey("fk_Driver")]
        public long? DriverId { get; set; }
        public virtual DriverMaster fk_Driver { get; set; }

        [Column("Remark"),MaxLength(500)]
        public string Remark { get; set; }
        [Column("TripLogId"), ForeignKey("fk_Triplog")]
        public long? TripLogId { get; set; }
        public virtual VehicleMovementLog fk_Triplog { get; set; }

        [Column("AdvanceTypeId"), ForeignKey("fk_AdvanceType")]
        public long? AdvanceTypeId { get; set; }
        public virtual VoucherType fk_AdvanceType { get; set; }

        [Column("ReferenceNo"), MaxLength(150), MinLength(3),Index("IX_TripAdvanceLog_ReferenceNo",IsUnique = true,Order = 0)]
        public string ReferenceNo { get; set; } 
        
        [Column("VoucherId")]
        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        [Column("ActualVCHId")]
        public long? ActualVCHId { get; set; }
        [ForeignKey("ActualVCHId")]
        public virtual Voucher fk_ActualVCH { get; set; }


        [Column("GroupVoucherId")]
        public long? GroupVoucherId { get; set; }
        [ForeignKey("GroupVoucherId")]
        public virtual Voucher fk_GroupVoucher { get; set; }

        [NotMapped]
        public decimal Amount {
            get
            {
                return this.FuelAmount>0?this.FuelAmount :this.CashAmount;
            }
        }
        public virtual List<TripExpenseLog> FuelExpanses { get; set; }
        public decimal BalanceQty { get; set; }
        public bool IsBulkEntry { get; set; }
        /// <summary>
        /// Gets or sets the settled reference identifier.
        /// <remarks>
        /// Used in case advances to be settled other than those settle in Trip Settlement Form
        /// </remarks>
        /// </summary>
        /// <value>The settled reference identifier.</value>
        public long? SettledRefId { get; set; }
        [ForeignKey("SettledRefId")]
        public virtual TripAdvanceLog fk_SettledRefAdvance { get; set; }
        public virtual List<TripAdvanceLog> SettledAdvances { get; set; }

        public long? ViewId { get; set; }
        public long? ExpenseId { get; set; }
        [ForeignKey("ExpenseId")]
        public virtual ExpenseMaster fk_Expense { get; set; }

        public long? PaidInId { get; set; }
        [ForeignKey("PaidInId")]
        public virtual ConstantValue fk_PaidIn { get; set; }
        [MaxLength(100)]
        public string Ref1 { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        [MaxLength(255)]
        public string BatchId { get; set; }

        public long? VDRId { get; set; }
        [ForeignKey("VDRId")]
        public virtual VoucherDetailReference fk_VDR { get; set; }
        public TripAdvanceLog Clone()
        {
            return (TripAdvanceLog) this.MemberwiseClone();
        }
        #region Disbursment Properties
        public long? ParentAdvanceLogId { get; set; }
        [ForeignKey("ParentAdvanceLogId")]
        public virtual TripAdvanceLog fk_ParentAdvanceLog { get; set; }
        public DateTime? RequestDate { get; set; }
        public decimal RequestQty { get; set; }
        public decimal RequestAmount { get; set; }
        public string RequestRemark { get; set; }
        public long? RequestStatusId { get; set; }
        [ForeignKey("RequestStatusId")]
        public virtual ConstantValue fk_RequestStatus { get; set; }

        #endregion
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (SettlementId.GetValueOrDefault() > 0 && TripLogId.GetValueOrDefault()==0)
            {
                if (this.AdvanceTypeId == 1|| this.AdvanceTypeId == 2)
                {
                    yield return new ValidationResult($"Trip Advance No {ReferenceNo} is not mapped to any TripLog");
                }
            }
            var includedTypes=new List<long?>(){1,2,3,15,16,13,11,88};
            if (this.VehicleId.GetValueOrDefault() == 0 && this.HireVehicleId.GetValueOrDefault() == 0&& includedTypes.Contains(AdvanceTypeId))
            {
                yield return new ValidationResult($"Missing Vehicle on Advance Ref No {this.ReferenceNo}");
            }
            if (VoucherDate == null)
            {
                VoucherDate = AdvanceDate;
            }
            if (RequestDate == null)
            {
                RequestDate = AdvanceDate;
            }
            if (BasicAmt <= 0&&(IGSTAmt+CGSTAmt+SGSTAmt)<=0)
            {
                BasicAmt = Amount;
            }
            if (this.CreditAccountId.GetValueOrDefault(0) == 0)
            {
                yield return new ValidationResult($"Credit Account is missing on Advance Ref No {this.ReferenceNo}");
            }
            if (this.DebitAccountId.GetValueOrDefault(0) == 0)
            {
                yield return new ValidationResult($"Debit Account is missing on Advance Ref No {this.ReferenceNo}");
            }
            if (this.DriverId.GetValueOrDefault(0) == 0&&this.VehicleId.GetValueOrDefault()>0&&AdvanceTypeId!=88)
            {
                yield return new ValidationResult($"Driver is missing on Advance Ref No {this.ReferenceNo}");
            }
            if (string.IsNullOrWhiteSpace(VoucherNo)||VoucherNo.Length<3)
            {
                yield return new ValidationResult("Voucher Number should be greater than 2 characters");
            }

            if (this.AdvanceTypeId == 3 && this.FuelQty > 0 && this.Id == 0)
            {
                this.BalanceQty = this.FuelQty;
            }
            if (RoundUp == null)
            {
                this.RoundUp = 0;
            }
        }
        #region HS Advance Properties
        //TODOD:Need to create new Voucher Types for Hire Payments //Advance/Adjustment//Balance Payment//On Account
        //[Column("AdvTypeId")] 
        //public long? AdvTypeId { get; set; }
        //[ForeignKey("AdvTypeId")]
        //public virtual ConstantValue fk_AdvanceType { get; set; }
        [Column("HireVehicleId")]
        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }
        #endregion
        /// <summary>
        /// Allow to Categorize Advances
        /// </summary>
        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_Ref1 { get; set; }
        [MaxLength(200)]
        public string ThirdPartyRefNo { get; set; }
        public string Data { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> DataView
        {
            get
            {
                try
                {
                    if (Data == "{}") Data = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((Data ?? (Data = "[]"))));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }
               
            }
            set
            {
                _dt = value;
                Data = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }
        }        
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((Data ?? "{}") == "{}") Data = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((Data ?? (Data = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                Data = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                Data = "[]";
            }
        }
        public DateTime? APRLDateTime { get; set; }
        public string APRLRemark { get; set; }
        public long? APRLSID { get; set; }
        public long? APRLUserId { get; set; }
        [Column("IsAPRLRequired")]
        public bool IsAutoAPRL { get; set; } = false;

    }
}
