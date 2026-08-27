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
    [Table("tTyreLogExtraInfo")]
    public class TyreLogExtraInfo : AuditableEntity,IValidatableObject
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

        public long? ProvisionalAcId { get; set; }
        [ForeignKey("ProvisionalAcId")]
        public virtual Ledger fk_ProvisionalAc { get; set; }

        [MaxLength(100), StationaryCheck, Required, MinLength(5), Index("IX_TyreLogExtraInfo_VoucherNo", IsUnique = true)]
        public string VoucherNo { get; set; }
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

        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public DateTime VoucherDate { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }

        public long? ORMId { get; set; }
        [ForeignKey("ORMId")]
        public virtual ORMLog fk_ORM { get; set; }

        public virtual List<TyreLog> TyreLogs { get; set; }

        [Column("GatepassNo"), MaxLength(100)]
        public string GatepassNo { get; set; }

        [Column("GatepassType"), MaxLength(10)]
        public string GatepassType { get; set; }

        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        public long? TubeHSNCodeId { get; set; }
        [ForeignKey("TubeHSNCodeId")]
        public virtual TaxServiceType fk_TubeHSNCode { get; set; }
        public long? FlapHSNCodeId { get; set; }
        [ForeignKey("FlapHSNCodeId")]
        public virtual TaxServiceType fk_FlapHSNCode { get; set; }
        public long? OtherHSNCodeId { get; set; }
        [ForeignKey("OtherHSNCodeId")]
        public virtual TaxServiceType fk_OtherHSNCode { get; set; }

        public long? IGSTACId { get; set; }
        [ForeignKey("IGSTACId")]
        public virtual Ledger fk_IGSTAC { get; set; }

        public long? CGSTACId { get; set; }
        [ForeignKey("CGSTACId")]
        public virtual Ledger fk_CGSTAC { get; set; }

        public long? SGSTACId { get; set; }
        [ForeignKey("SGSTACId")]
        public virtual Ledger fk_SGSTAC { get; set; }
        public long? ViewId { get; set; }
        public long? TCSAccountId { get; set; }
        [ForeignKey("TCSAccountId")]
        public virtual Ledger fk_TCSAccount { get; set; }

        public long? RoundOffAccId { get; set; }
        [ForeignKey("RoundOffAccId")]
        public virtual Ledger fk_RoundOffAcc { get; set; }
        public long? PostDiscountAcId { get; set; }
        [ForeignKey("PostDiscountAcId")]
        public virtual Ledger fk_PostDiscountAc { get; set; }
        public long? OtherAcId { get; set; }
        [ForeignKey("OtherAcId")]
        public virtual Ledger fk_OtherAcId { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal PostDiscountAmt { get; set; }
        public decimal OtherChgAmt { get; set; }
        public decimal RoundOffAmt { get; set; }
        [Precision(28, 4)]
        public decimal TCSRate { get; set; }
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }

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
            if (VoucherDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed", new[] { "VoucherDate" });
            }
        }
    }
}