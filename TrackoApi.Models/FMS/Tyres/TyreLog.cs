using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;

using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{

    [Table("tTyreLog")]
    public class TyreLog : AuditableEntity, IValidatableObject/*,IAprovalEntity*/
    {
        [MaxLength(100), Required]
        public string VoucherNo { get; set; }
        [Required]
        public DateTime VoucherDate { get; set; }
        public long CreditAccountId { get; set; }
        [ForeignKey("CreditAccountId")]
        public virtual Ledger fk_CreditAccount { get; set; }
        public long DebitAccountId { get; set; }
        [ForeignKey("DebitAccountId")]
        public virtual Ledger fk_DebitAccount { get; set; }
        [Required]
        public long VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }

        [Column("VoucherId")]
        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }

        [Column("TyreId"), Required, ForeignKey("fk_Tyre")]
        public long TyreId { get; set; }
        public virtual TyreMaster fk_Tyre { get; set; }
        [MaxLength(100), Required]
        public string TyreNo { get; set; }

        [Column("VehicleId"), ForeignKey("fk_Vehicle")]
        public long? VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        // [Column("PurchaseOrderId"), ForeignKey("fk_PurchaseOrder")]
        // public long? PurchaseOrderId { get; set; }
        // public virtual PurchaseOrder fk_PurchaseOrder { get; set; }
        [Column("POLogId")]
        public long? POLogId { get; set; }
        [ForeignKey(nameof(POLogId))]
        public PurchaseOrderLog fk_POLog { get; set; }

        [Column("RubberTypeId"), ForeignKey("fk_RubberType")]
        public long? RubberTypeId { get; set; }
        public virtual BrandMaster fk_RubberType { get; set; }

        [Column("ReasonId"), ForeignKey("fk_Reason")]
        public long? ReasonId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ reason.
        /// Of Type 1098
        /// </summary>
        /// <value>The FK_ reason.</value>
        public virtual GenericMaster fk_Reason { get; set; }

        [Column("JobsheetId"), ForeignKey("fk_Jobsheet")]
        public long? JobsheetId { get; set; }
        public virtual VehicleMovementLog fk_Jobsheet { get; set; }

        [Column("TyreStatusId"), ForeignKey("fk_TyreStatus")]
        public long TyreStatusId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ tyre status.
        /// Of Type 56
        /// </summary>
        /// <value>The FK_ tyre status.</value>
        public virtual ConstantValue fk_TyreStatus { get; set; }

        [Column("NextUseId"), ForeignKey("fk_NextUse")]
        public long? NextUseId { get; set; }

        public virtual ConstantValue fk_NextUse { get; set; }
        [Column("TyreLife")]
        public int TyreLife { get; set; } = -1;
        /// <summary>
        /// Gets or sets the km reading.
        /// OnKm/OutKm
        /// </summary>
        /// <value>The km reading.</value>
        [Column("KmReading")]
        public long KmReading { get; set; } = 0;
        /// <summary>
        /// Gets or sets the km run.
        /// DifferenceKM
        /// </summary>
        /// <value>The km run.</value>
        [Column("KmRun")]
        public long KmRun { get; set; } = 0;
        [Column("IsStepney")]
        public bool IsStepney { get; set; }
        [Column("IsRemoulded")]
        public bool IsRemoulded { get; set; }

        [Column("IssueReceiptId")]
        public long? IssueReceiptId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ issue receipt.
        /// Recording issued tyre log id in lieu of received tyre log
        /// </summary>
        /// <value>The FK_ issue receipt.</value>
        [ForeignKey("IssueReceiptId")]
        public virtual TyreLog fk_IssueReceipt { get; set; }

        [Column("ParentLogId")]
        public long? PreviousLogId { get; set; }
        [ForeignKey("ParentLogId")]
        public virtual TyreLog fk_PreviousLog { get; set; }
        [Column("NextLogId")]
        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public TyreLog fk_NextLog { get; set; }

        [Column("Remark"), MaxLength(255)]
        public string Remark { get; set; }

        public int WarrantyKm { get; set; }
        public int WarrantyDays { get; set; }
        public decimal Rate { get; set; } = 0;
        public decimal TubeRate { get; set; } = 0;
        public decimal FlapRate { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TubeDiscountPercent { get; set; } = 0;
        public decimal TubeDiscountAmount { get; set; } = 0;
        public decimal FlapDiscountPercent { get; set; } = 0;
        public decimal FlapDiscountAmount { get; set; } = 0;
        //public long? TaxServiceTypeId { get; set; }
        //[ForeignKey("TaxServiceTypeId")]
        //public virtual TaxServiceType fk_TaxServiceType { get; set; }
        public decimal OtherAmount { get; set; } = 0;
        public decimal TubeOtherAmount { get; set; } = 0;
        public decimal FlapOtherAmount { get; set; } = 0;
        public decimal SubTotal { get; set; } = 0;
        public decimal TubeSubTotal { get; set; } = 0;
        public decimal FlapSubTotal { get; set; } = 0;

        public decimal CGSTPercent { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;

        public decimal SGSTPercent { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;

        public decimal IGSTPercent { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;

        public decimal TubeCGSTPercent { get; set; } = 0;
        public decimal TubeCGSTAmount { get; set; } = 0;

        public decimal TubeSGSTPercent { get; set; } = 0;
        public decimal TubeSGSTAmount { get; set; } = 0;

        public decimal TubeIGSTPercent { get; set; } = 0;
        public decimal TubeIGSTAmount { get; set; } = 0;

        public decimal FlapCGSTPercent { get; set; } = 0;
        public decimal FlapCGSTAmount { get; set; } = 0;

        public decimal FlapSGSTPercent { get; set; } = 0;
        public decimal FlapSGSTAmount { get; set; } = 0;

        public decimal FlapIGSTPercent { get; set; } = 0;
        public decimal FlapIGSTAmount { get; set; } = 0;

        public decimal TyreTotalAmount { get; set; } = 0;
        public decimal TubeTotalAmount { get; set; } = 0;
        public decimal FlapTotalAmount { get; set; } = 0;

        public decimal RoundUpAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        public bool CalVat { get; set; } = false;
        public bool CalOthAmt { get; set; } = false;
        public decimal ScrapCost { get; set; } = 0;
        public decimal EstScrapValue { get; set; } = 0;
        public decimal TransferPrice { get; set; } = 0;
        public long? MechanicId { get; set; }
        [ForeignKey("MechanicId")]
        public virtual GenericMaster fk_Mechanic { get; set; }

        public long? ExtraInfoId { get; set; }
        [ForeignKey("ExtraInfoId")]
        public virtual TyreLogExtraInfo ExtraInfo { get; set; }
        public int AirPressure { get; set; }
        public long? TyreCheckId { get; set; }
        [ForeignKey("TyreCheckId")]
        public virtual TyreCheck fk_TyreCheck { get; set; }

        public long? BillExtraInfoId { get; set; }
        [ForeignKey("BillExtraInfoId")]
        public virtual TyreLogExtraInfo fk_Bill { get; set; }

        [IgnoreDataMember, NotMapped]
        public bool IgnoreValidation { get; set; } = false;
        [ForeignKey("GatePassId")]
        public virtual FleetGatePass fk_GatePass { get; set; }
        public long? GatePassId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VoucherDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed", new[] { "VoucherDate" });
            }
            if (ObjectState==ObjectState.Added||ObjectState==ObjectState.Modified)
            {
                if (NetAmount>0)
                {
                    if (Rate - DiscountAmount + OtherAmount != SubTotal)/*Tyre*/
                    {
                        yield return new ValidationResult($"SubTotal for Tyre No {TyreNo} has invalid value {SubTotal}. It should be {Rate - DiscountAmount + OtherAmount}", new[] { "SubTotal" });
                    }
                    if (TubeRate - TubeDiscountAmount + TubeOtherAmount != TubeSubTotal)
                    {
                        yield return new ValidationResult($"SubTotal of Tube for Tyre No {TyreNo} has invalid value. It should be {TubeRate - TubeDiscountAmount + TubeOtherAmount}", new[] { "TubeSubTotal" });
                    }
                    if (FlapRate - FlapDiscountAmount + FlapOtherAmount != FlapSubTotal)
                    {
                        yield return new ValidationResult($"SubTotal of Flap for Tyre No {TyreNo} has invalid value{FlapSubTotal}. It should be {FlapRate - FlapDiscountAmount + FlapOtherAmount}", new[] { "FlapSubTotal" });
                    }
                    if (SubTotal + (CalVat ? 0 : CGSTAmount + SGSTAmount + IGSTAmount) != TyreTotalAmount)
                    {
                        yield return new ValidationResult($"Tyre Item Total of Tyre No {TyreNo} has invalid value {TyreTotalAmount}. It should be {SubTotal + CGSTAmount + SGSTAmount + IGSTAmount}", new[] { "TyreTotalAmount" });
                    }
                    if (TubeSubTotal + (CalVat ? 0 : TubeCGSTAmount + TubeSGSTAmount + TubeIGSTAmount) != TubeTotalAmount)
                    {
                        yield return new ValidationResult($"Tube Item Total of Tyre No {TyreNo} has invalid value{TubeTotalAmount}. It should be {TubeSubTotal + CGSTAmount + SGSTAmount + IGSTAmount}", new[] { "TubeTotalAmount" });
                    }
                    if (FlapSubTotal + (CalVat ? 0 : FlapCGSTAmount + FlapSGSTAmount + FlapIGSTAmount) != FlapTotalAmount)
                    {
                        yield return new ValidationResult($"Flap Item Total of Tyre No {TyreNo} has invalid value{FlapTotalAmount}. It should be {FlapSubTotal + CGSTAmount + SGSTAmount + IGSTAmount}", new[] { "FlapTotalAmount" });
                    }

                    if ((TyreTotalAmount + TubeTotalAmount + FlapTotalAmount + RoundUpAmount) != NetAmount)
                    {
                        yield return new ValidationResult($"NetAmount Rs.{NetAmount} for Tyre No {TyreNo} has invalid value. It should be Rs. {TyreTotalAmount + TubeTotalAmount + FlapTotalAmount}");
                    }
                }
                //if (VoucherTypeId==34 && (IssueReceiptId.GetValueOrDefault(0)==0&&fk_IssueReceipt==null)&& !IgnoreValidation)
                //{
                //    yield return new ValidationResult("Receipt is Required against Issue", new[] { "IssueReceiptId" });
                //}
                //if (VoucherTypeId == 35 && IssueReceiptId.GetValueOrDefault(0) == 0 && ObjectState == ObjectState.Modified&& !IgnoreValidation)
                //{
                //    yield return new ValidationResult("Tyre Issue is Required against Tyre Receipt", new[] { "IssueReceiptId" });
                //}
                if (VoucherTypeId == 27)
                {
                    if (TyreLife != 0) yield return new ValidationResult("Initial Life on Tyre Purchase should be Zero", new[] { "TyreLife" });
                }
                if (VoucherTypeId == 29 && TyreLife <= 0)
                {
                    yield return new ValidationResult("Retreated Tyre's Life should always greater than Zero", new[] { "TyreLife" });
                }
                if (string.IsNullOrWhiteSpace(TyreNo))
                {
                    yield return new ValidationResult("Tyre No is Required", new[] { "TyreNo" });
                }
                if (Id == IssueReceiptId && IssueReceiptId.GetValueOrDefault(0) != 0)
                {
                    yield return new ValidationResult("Issue can't be Receipt", new[] { "IssueReceiptId" });
                }
            }
            
        }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public long ReceiptMonth { get; set; } = 0;
        /// <summary>
        /// Sum of GPS Km
        /// Sum(PerDayKM) from DailKM table
        /// </summary>
        public long GPSKm { get; set; }
        /// <summary>
        /// Sum of JobCard KM
        /// SUM(JobCardKM) from tVehicleMovement Table(Trip Type 1159)
        /// </summary>
        public long JobCardKm { get; set; }
        /// <summary>
        /// Sum of TripLog KM
        /// SUM(JobCardKM) from tVehicleMovement Table(Trip Type 1158,1453)
        /// </summary>
        public long TLKm { get; set; }
        /// <summary>
        /// Sum of Odo Meter KM that is entred by user when receving tyre from Vehicle
        /// tyre receipt odometer km -(minus) tyre issue odo meter km
        /// </summary>
        public long OdoKm { get; set; }
        /// <summary>
        /// 1483	Odo Meter KM (Manual)
        /// 1484	TripLog
        /// 1485	JobCard
        /// 1730	GPS Km
        /// </summary>
        public long? KMSourceId { get; set; }
        [ForeignKey(nameof(KMSourceId))]
        public virtual ConstantValue fk_KMSource { get; set; }
        //public DateTime? APRLDateTime { get; set; }
        //public string APRLRemark { get; set; }
        //public long? APRLSID { get; set; }
        //public long? APRLUserId { get; set; }
        //public bool IsAutoAPRL { get; set; } = false;
        public bool IsException { get; set; } = false;
        
              
        public long? TSLId { get; set; }
        [ForeignKey("TSLId")]
        public virtual TransactionSupportLog fk_TSL { get; set; }
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            get
            {
                try
                {
                    if (JsonData == "{}") JsonData = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData ?? (JsonData = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                JsonData = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }


        }
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((JsonData ?? "{}") == "{}") JsonData = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((JsonData ?? (JsonData = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                JsonData = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                JsonData = "[]";
            }
        }
    }
}
