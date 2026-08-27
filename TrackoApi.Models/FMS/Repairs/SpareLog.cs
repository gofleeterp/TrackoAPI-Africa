using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Base.Attributes;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.FMS.Repairs;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.ViewModels.Global;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace TrackoApi.Models.FMS
{
    [Table("tSpareLog")]
    public class SpareLog : AuditableEntity,IValidatableObject
    {
        public long? TSLId { get; set; }
        [ForeignKey("TSLId")]
        public virtual TransactionSupportLog fk_TSL { get; set; }

        [MaxLength(100)]
        public string VoucherNo { get; set; }
        [Required]
        public DateTime VoucherDate { get; set; }
        [Column("CrAccountId")]
        public long? CrAccountId { get; set; }
        [ForeignKey("CrAccountId")]
        public virtual Ledger fk_CrAccount{ get; set; }
        [Column("DrAccountId")]
        public long? DrAccountId { get; set; }
        [ForeignKey("DrAccountId")]
        public virtual Ledger fk_DrAccount { get; set; }


        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }

        [Column("VoucherId")]
        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }

        [Column("RefId")]
        public long? ReferenceId { get; set; }
        [ForeignKey("RefId")]
        public virtual SpareLog fk_Reference { get; set; }

        // [Column("PurchaseOrderId"), ForeignKey("fk_PurchaseOrder")]
        // public long? PurchaseOrderId { get; set; }
        // public virtual PurchaseOrder fk_PurchaseOrder { get; set; }
        [Column("POLogId")]
        public long? POLogId { get; set; }
        [ForeignKey(nameof(POLogId))]
        public PurchaseOrderLog fk_POLog { get; set; }

        [Column("JobCardId"), ForeignKey("fk_JobCard")]
        public long? JobCardId { get; set; }
        public virtual VehicleMovementLog fk_JobCard{ get; set; }
        [Column("VehicleId")]
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("HireVehicleId")]
        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }

        [Column("SparePartId"), Required]
        public long SparePartId { get; set; }
        [ForeignKey("SparePartId")]
        public virtual SpareMaster fk_Spare { get; set; }
        public long? MakeId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ make.
        /// </summary>
        /// <value>The FK_ make.</value>
        [ForeignKey("MakeId")]
        public virtual GenericMaster fk_Make { get; set; }
        public long? BinId { get; set; }
        [ForeignKey("BinId")]
        public virtual StoreBinMaster Bin { get; set; }
        public int WarrantyKm { get; set; }
        public int ODOKm { get; set; }
        public int WarrantyDays { get; set; }
        [Precision(28, 4)]
        public decimal Qty { get; set; } = 0;
        /// <summary>
        /// Gets or sets the deposited qty.
        /// Deposited against Issue
        /// </summary>
        /// <value>The deposited qty.</value>
        /// 
        [Precision(28, 4)]
        public decimal DepositedQty { get; set; } = 0;
        [Precision(28, 4)]
        public decimal Rate { get; set; } = 0;
        public long? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual UnitMaster fk_Unit { get; set; }
        public decimal Amount { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        //public decimal VatPercent { get; set; } = 0;
        //public decimal VatAmount { get; set; } = 0;

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }
        public decimal CGSTRate { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTRate { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;
        public decimal IGSTRate { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;


        public decimal SubTotal { get; set; } = 0;
        public decimal PostDisount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal OtherAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        [Column("Remarks"), MaxLength(255)]
        public string Remark { get; set; }
        public long? ExtraInfoId { get; set; }
        [ForeignKey("ExtraInfoId")]

        public virtual SpareLogExtraInfo ExtraInfo { get; set; }
        public virtual  List<SpareLog> IssuedLogs { get; set; }

        [Precision(28, 4)]
        public decimal StockQty { get; set; } = 0;

        public long? FittingPositionId { get; set; }
        [ForeignKey("FittingPositionId")]
        public virtual GenericMaster fk_FittingPosition { get; set; }

        public long? UnitTypeId { get; set; }
        [ForeignKey("UnitTypeId")]
        public virtual ConstantValue fk_UnitType { get; set; }

        public long? MechanicId { get; set; }
        [ForeignKey("MechanicId")]
        public virtual GenericMaster fk_Mechanic { get; set; }

        public long? BillExtraInfoId { get; set; }
        [ForeignKey("BillExtraInfoId")]
        public virtual SpareLogExtraInfo fk_Bill { get; set; }

        [ForeignKey("GatePassId")]
        public virtual FleetGatePass fk_GatePass { get; set; }
        public long? GatePassId { get; set; }
        

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VoucherDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed", new[] { "VoucherDate" });
            }
            if (StockQty < 0 || StockQty > Qty)
            {
                yield return new ValidationResult("StockQty cannot be [gt] Actual Qty nor [lt] Zero.", new[] { "StockQty" });
            }
        }
        [MaxLength(200)]
        public string BatchId { get; set; }
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(JsonData)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData)): _dt;
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
    [Table("tSpareLogExtraInfo")]
    public class SpareLogExtraInfo:AuditableEntity,IValidatableObject/*,IAprovalEntity*/
    {
        [MaxLength(50)]
        public string TPT_RequestId { get; set; }

        [MaxLength(100), StationaryCheck]
        public string DocNo { get; set; }

        public long? PartyGSTOfficeId { get; set; }
        [ForeignKey("PartyGSTOfficeId")]
        public virtual LedgerOffice fk_PartyGSTOffice { get; set; }

        public DateTime DocDate { get; set; }
        public long? DrAccountId { get; set; }
        [ForeignKey("DrAccountId")]
        public virtual Ledger fk_DrAccount { get; set; }
        public decimal DrAmount { get; set; }
        public long? CrAccountId { get; set; }
        [ForeignKey("CrAccountId")]
        public virtual Ledger fk_CrAccount { get; set; }
        public decimal CrAmount { get; set; }
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        public long? GroupVoucherId { get; set; }
        [ForeignKey("GroupVoucherId")]
        public virtual Voucher fk_GroupVoucher { get; set; }
        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }
        public long? OrmId { get; set; }
        [ForeignKey("OrmId")]
        public virtual ORMLog fk_ORM { get; set; }
        public bool CalculateVat { get; set; } = false;
        public long? OtherChargeRatioId { get; set; }
        [ForeignKey("OtherChargeRatioId")]
        public virtual ConstantValue OtherChargeRatio { get; set; }
        [Column("VendorRefNo"),MaxLength(100)]
        public string VendorReferenceNo { get; set; }
        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }
        public long? ProvisionalAcId { get; set; }
        [ForeignKey("ProvisionalAcId")]
        public virtual Ledger fk_ProvisionalAc { get; set; }

        [Column("GatepassNo"), MaxLength(50)]
        public string GatepassNo { get; set; }

        [Column("GatepassType"), MaxLength(50)]
        public string GatepassType { get; set; }
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }

        [MaxLength(100)]
        public string ChallanSlipNo { get; set; }
        public DateTime? ChallanSlipDate { get; set; }
        public virtual List<SpareLog> SpareLogs { get; set; }
        public virtual List<SpareLog> BillSpareLogs { get; set; }
        public long? ViewId { get; set; }
        [MaxLength(200)]
        public string BatchId { get; set; }
        public long? RelatedVoucherId { get; set; }
        [ForeignKey("RelatedVoucherId")]
        public virtual Voucher fk_RelatedVoucher { get; set; }
        [Precision(28, 4)]
        public decimal Qty { get; set; }
        public long? IGSTACId { get; set; }
        [ForeignKey("IGSTACId")]
        public virtual Ledger fk_IGSTAC { get; set; }
        public decimal IGSTPercent { get; set; }
        public decimal IGSTAmount{ get; set; }
        public long? CGSTACId { get; set; }
        [ForeignKey("CGSTACId")]
        public virtual Ledger fk_CGSTAC { get; set; }
        public decimal CGSTPercent { get; set; }
        public decimal CGSTAmount { get; set; }
        public long? SGSTACId { get; set; }
        [ForeignKey("SGSTACId")]
        public virtual Ledger fk_SGSTAC { get; set; }
        public decimal SGSTPercent { get; set; }
        public decimal SGSTAmount { get; set; }
        public long? PostDiscountAcId { get; set; }
        [ForeignKey("PostDiscountAcId")]
        public virtual Ledger fk_PostDiscountAc { get; set; }
        [SqlDefaultValue(DefaultValue = "0")]
        public decimal PostDiscAmount { get; set; }
        public long? OtherAccountId { get; set; }
        [ForeignKey("OtherAccountId")]
        public virtual Ledger fk_OtherAccount { get; set; }
        public decimal OtherAmount { get; set; }
        public long? RoundOffAcId { get; set; }
        [ForeignKey("RoundOffAcId")]
        public virtual Ledger fk_RoundOffAc { get; set; }
        [SqlDefaultValue(DefaultValue = "0")]
        public decimal RoundOff { get; set; }
        public long? TDSAccountId { get; set; }
        [ForeignKey("TDSAccountId")]
        public virtual Ledger fk_TDSAccount { get; set; }

        public long? TDSVoucherId { get; set; }
        [ForeignKey("TDSVoucherId")]
        public virtual Voucher fk_TDSVoucher { get; set; }

        public long? TCSAccountId { get; set; }
        [ForeignKey("TCSAccountId")]
        public virtual Ledger fk_TCSAccount { get; set; }
        public decimal TCSAmount { get; set; }
        [Precision(28, 4)]
        public decimal TCSRate { get; set; }
        [Precision(28, 4)]
        public decimal TDSRate { get; set; }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DocDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed", new[] { "DocDate" });
            }
        }
        //public DateTime? APRLDateTime { get; set; }
        //public string APRLRemark { get; set; }
        //public long? APRLSID { get; set; }
        //public long? APRLUserId { get; set; }
        //public bool IsAutoAPRL { get; set; } = false;
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(JsonData)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData)): _dt;
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

        public decimal TDSAmount { get; set; }


        /*Purpose ZRA*/
        [Column("BillingCategory"), MaxLength(15)]
        public string BillingCategory { get; set; }

        [Column("DestnCountryId")]
        public long? DestnCountryId { get; set; }
        [ForeignKey("DestnCountryId")]
        public virtual GenericMaster fk_DestnCountry { get; set; }

        //public long? TaxRateId { get; set; }
        //[ForeignKey("TaxRateId")]
        //public virtual TaxRateMaster fk_TaxRate { get; set; }

        [Column("ReasonId")]
        public long? ReasonId { get; set; }
        [ForeignKey("ReasonId")]
        public virtual GenericMaster fk_Reason { get; set; }

        [MaxLength(500)]
        public string OtherReason { get; set; }

        public int? PrintCount { get; set; }

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
