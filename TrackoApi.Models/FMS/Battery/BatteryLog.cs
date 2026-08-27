using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Base.Attributes;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tBatteryLog")]
    public class BatteryLog : AuditableEntity,IValidatableObject
    {
        public long? TSLId { get; set; }
        [ForeignKey("TSLId")]
        public virtual TransactionSupportLog fk_TSL { get; set; }

        [MaxLength(100),Required]
        public string DocNo { get; set; }
        [Required]
        public DateTime DocDate { get; set; }
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

        [Column("BatteryId"), Required, ForeignKey("fk_Battery")]
        public long BatteryId { get; set; }
        public virtual BatteryMaster fk_Battery { get; set; }

        [MaxLength(100),Required]
        public string BatterySerialNo { get; set; }

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

        [Column("ReasonId"), ForeignKey("fk_Reason")]
        public long? ReasonId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ reason.
        /// Of Type 
        /// </summary>
        /// <value>The FK_ reason.</value>
        public virtual GenericMaster fk_Reason { get; set; }

        [Column("JobsheetId"), ForeignKey("fk_Jobsheet")]
        public long? JobsheetId { get; set; }
        public virtual VehicleMovementLog fk_Jobsheet { get; set; }

        [Column("BatteryStatusId"), ForeignKey("fk_BatteryStatus")]
        public long BatteryStatusId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ Battery status.
        /// Of Type 56
        /// </summary>
        /// <value>The FK_ Battery status.</value>
        public virtual ConstantValue fk_BatteryStatus { get; set; }

        [Column("NextUseId"), ForeignKey("fk_NextUse")]
        public long? NextUseId { get; set; }
        public virtual ConstantValue fk_NextUse { get; set; }
        [Column("BatteryLife")]
        public int BatteryLife { get; set; } = -1;

     

        [Column("IsRefurbish")]
        public bool IsRefurbish { get; set; }

        [Column("IssueReceiptId")]
        public long? IssueReceiptId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ issue receipt.
        /// Recording issued Battery log id in lieu of received Battery log
        /// </summary>
        /// <value>The FK_ issue receipt.</value>
        [ForeignKey("IssueReceiptId")]
        public virtual BatteryLog fk_IssueReceipt { get; set; }

        [Column("ParentLogId")]
        public long? PreviousLogId { get; set; }
        [ForeignKey("ParentLogId")]
        public virtual BatteryLog fk_PreviousLog { get; set; }

        [Column("NextLogId")]
        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public BatteryLog fk_NextLog { get; set; }

        [Column("Remark"), MaxLength(255)]
        public string Remark { get; set; }
        public int WarrantyDays { get; set; }
        public decimal Rate { get; set; } = 0;        
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        public decimal CGSTPercent { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTPercent { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;
        public decimal IGSTPercent { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;

        public decimal SubTotal { get; set; } = 0;
        public decimal OtherAmount { get; set; } = 0;
        public decimal RoundAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        
        public decimal NetAmount_MNC { get; set; } = 0;

        public bool CalVat { get; set; } = false;
        public bool CalOthAmt { get; set; } = false;
        public decimal ScrapCost { get; set; } = 0;
        public decimal TransferPrice { get; set; } = 0;
        public long? MechanicId { get; set; }
        [ForeignKey("MechanicId")]
        public virtual GenericMaster fk_Mechanic { get; set; }

        public long? ExtraInfoId { get; set; }
        [ForeignKey("ExtraInfoId")]

        public virtual BatteryLogExtraInfo ExtraInfo { get; set; }
        
        public long? BatteryCheckId { get; set; }
        [ForeignKey("BatteryCheckId")]
        public virtual BatteryCheck fk_BatteryCheck { get; set; }

        public long? BillExtraInfoId { get; set; }
        [ForeignKey("BillExtraInfoId")]
        public virtual BatteryLogExtraInfo fk_Bill { get; set; }


        [IgnoreDataMember, NotMapped]
        public bool IgnoreValidation { get; set; } = false;
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
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DocDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed Battery No {BatterySerialNo}", new[] { "DocDate" });
            }
            if (Rate-DiscountAmount+(CalOthAmt ? 0 : OtherAmount )!= SubTotal)
            {
                yield return new ValidationResult($"SubTotal has invalid value. It should be {Rate - DiscountAmount + (CalOthAmt ? 0 : OtherAmount)}",new[] { "SubTotal" });
            }
            if (SubTotal + (CalVat ? 0 : CGSTAmount + SGSTAmount + IGSTAmount)+RoundAmount != NetAmount)
            {
                yield return new ValidationResult($"NetAmount has invalid value. It should be {SubTotal + RoundAmount + ((CalVat ? 0 : CGSTAmount + SGSTAmount + IGSTAmount))}", new[] { "NetAmount" });
            }
            if (VoucherTypeId == 27)
            {
                if (BatteryLife != 0) yield return new ValidationResult("Initial Life on Battery Purchase should be Zero", new[] { "BatteryLife" });
            }
            if (VoucherTypeId == 29 && BatteryLife <= 0)
            {
                yield return new ValidationResult("Refurbished Battery's Life should always greater than Zero", new[] { "BatteryLife" });
            }
            if (string.IsNullOrWhiteSpace(BatterySerialNo))
            {
                yield return new ValidationResult("Battery Serial No is Required", new[] {"BatterySerialNo"});
            }
            if (VoucherTypeId==34 && (IssueReceiptId.GetValueOrDefault(0)==0&&fk_IssueReceipt==null))
            {
                yield return new ValidationResult("Receipt is Required against Issue", new[] { "IssueReceiptId" });
            }
            if (VoucherTypeId == 35 && IssueReceiptId.GetValueOrDefault(0) == 0 && ObjectState == ObjectState.Modified)
            {
                yield return new ValidationResult("Battery Issue is Required against Battery Receipt", new[] { "IssueReceiptId" });
            }
            if (Id == IssueReceiptId&& IssueReceiptId.GetValueOrDefault(0)!=0)
            {
                yield return new ValidationResult("Issue can't be Receipt", new[] { "IssueReceiptId" });
            }
        }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public int? BatteryAge { get; set; } = 0;
        [ForeignKey("GatePassId")]
        public virtual FleetGatePass fk_GatePass { get; set; }
        public long? GatePassId { get; set; }
    }
    [Table("tBatteryLifePerf")]
    public class BatteryLifePerformanceLog:AuditableEntity,IValidatableObject
    {
        [Column(Order =0),Index("IX_BatteryPerformanceLog_BatteryId",IsUnique =true,Order =0)]
        public long BatteryId { get; set; }
        [ForeignKey("BatteryId")]
        public virtual BatteryMaster fk_Battery { get; set; }
        [Column(Order = 1), Index("IX_BatteryPerformanceLog_BatteryId", IsUnique = true, Order = 1)]
        public int Life { get; set; } = -1;
        public DateTime LifeStartDate { get; set; }
        public DateTime? LifeEndDate { get; set; }
        public long SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual Ledger fk_Supplier { get; set; }
        public decimal PurchaseAmount { get; set; } = 0;
        public decimal PreviousAge { get; set; } = 0;
        public decimal LifeAge { get; set; } = 0;
        public decimal CurrentAge { get; set; } = 0;
        public long? FirstIssueLogId { get; set; }
        [ForeignKey("FirstIssueLogId")]
        public virtual BatteryLog fk_FirstIssueLog { get; set; }
        public long? LastReceiptLogId { get; set; }
        [ForeignKey("LastReceiptLogId")]
        public virtual BatteryLog fk_LastReceiptLog { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((Life < 0))
            {
                yield return new ValidationResult("Invalid Battery Life Provided",new []{ "Life" });
            }
            if (Life > 0 && PreviousAge <= 0)
            {
                yield return new ValidationResult($"Previous Mileage should be greater then zero when Life is {Life}",new[] { "PreviousMileage" });
            }
            if (LifeEndDate.HasValue && LifeAge <= 0)
            {
               yield return new ValidationResult("When Battery Life is to end then Life Mileage should be greater than Zero",new []{ "LifeMileage" });
            }
        }
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

    [Table("tBatteryLogExtraInfo")]
    public class BatteryLogExtraInfo : AuditableEntity,IValidatableObject
    {
        [MaxLength(50)]
        public string TPT_RequestId { get; set; }
        public long? PartyGSTOfficeId { get; set; }
        [ForeignKey("PartyGSTOfficeId")]
        public virtual LedgerOffice fk_PartyGSTOffice { get; set; }
        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        public long? GroupVoucherId { get; set; }
        [ForeignKey("GroupVoucherId")]
        public virtual Voucher fk_GroupVoucher { get; set; }
        public bool CalVat { get; set; } = false;
        public bool CalOthAmt { get; set; } = false;
        [MaxLength(100)]
        public string VendorReferenceNo { get; set; }
        public long? TransitStoreId { get; set; }
        [ForeignKey("TransitStoreId")]
        public virtual Ledger fk_TransitStore { get; set; }
        [MaxLength(100), StationaryCheck, Required, MinLength(5), Index("IX_BatteryLogExtraInfo_VoucherNo", IsUnique = true)]
        public string DocNo { get; set; }
        [Required]
        public long VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }
        public long? DrAccountId { get; set; }
        [ForeignKey("DrAccountId")]
        public virtual Ledger fk_DrAccount { get; set; }
        public long? CrAccountId { get; set; }
        [ForeignKey("CrAccountId")]
        public virtual Ledger fk_CrAccount { get; set; }
        public long? TCSAccountId { get; set; }
        public long? RoundOffAcId { get; set; }
        [ForeignKey("RoundOffAcId")]
        public virtual Ledger fk_RoundOffAc { get; set; }
        [SqlDefaultValue(DefaultValue = "0")]
        public decimal RoundOffAmount { get; set; }
        public long? PostDiscountAcId { get; set; }
        [ForeignKey("PostDiscountAcId")]
        public virtual Ledger fk_PostDiscountAc { get; set; }
        [SqlDefaultValue(DefaultValue = "0")]
        public decimal PostDiscountAmount { get; set; }
        public long? OtherLedgerId { get; set; }
        [ForeignKey("OtherLedgerId")]
        public virtual Ledger fk_OtherLedger { get; set; }
        [SqlDefaultValue(DefaultValue ="0")]
        public decimal OtherAmount { get; set; }
        [ForeignKey("TCSAccountId")]
        public virtual Ledger fk_TCSAccount { get; set; }
        [Precision(28, 4)]
        public decimal TCSRate { get; set; }
        [Precision(28, 4)]
        public decimal TCSAmount { get; set; }
        //public virtual Ledger fk_TdSAccount { get; set; }
        //[Precision(28, 4)]
        //public decimal TdSRate { get; set; }
        //[Precision(28, 4)]
        //public decimal TdSAmount { get; set; }

        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        public DateTime DocDate { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }

        public virtual List<BatteryLog> BatteryLogs { get; set; }

        public long? ORMId { get; set; }
        [ForeignKey("ORMId")]
        public virtual ORMLog fk_ORM { get; set; }

        [Column("GatepassNo"), MaxLength(100)]
        public string GatepassNo { get; set; }

        [Column("GatepassType"), MaxLength(10)]
        public string GatepassType { get; set; }
        public long? ViewId { get; set; }

        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        public long? IGSTACId { get; set; }
        [ForeignKey("IGSTACId")]
        public virtual Ledger fk_IGSTAC { get; set; }

        public long? CGSTACId { get; set; }
        [ForeignKey("CGSTACId")]
        public virtual Ledger fk_CGSTAC { get; set; }

        public long? SGSTACId { get; set; }
        [ForeignKey("SGSTACId")]
        public virtual Ledger fk_SGSTAC { get; set; }
        public long? ProvisionalAcId { get; set; }
        [ForeignKey("ProvisionalAcId")]
        public virtual Ledger fk_ProvisionalAc { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;


        /*Purpose ZRA*/
        [Column("BillingCategory"), MaxLength(15)]
        public string BillingCategory { get; set; }

        [Column("DestnCountryId")]
        public long? DestnCountryId { get; set; }
        [ForeignKey("DestnCountryId")]
        public virtual GenericMaster fk_DestnCountry { get; set; }

        public long? TaxRateId { get; set; }
        [ForeignKey("TaxRateId")]
        public virtual TaxRateMaster fk_TaxRate { get; set; }


        [Column("ReasonId")]
        public long? ReasonId { get; set; }
        [ForeignKey("ReasonId")]
        public virtual GenericMaster fk_Reason { get; set; }

        [MaxLength(500)]
        public string OtherReason { get; set; }

        public int? PrintCount { get; set; }

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
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DocDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed Doc No {DocNo}", new[] { "DocDate" });
            }
        }
    }
}
