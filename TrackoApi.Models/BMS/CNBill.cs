using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.BMS
{
    [Table("tCNBillMaster")]
    public class CNBill : AuditableEntity,IValidatableObject/*,IAprovalEntity*/
    {
        private string _jsonBillLogs;
        public long BillOfficeId { get; set; }
        [ForeignKey("BillOfficeId")]
        public virtual OfficeMaster fk_BillOffice { get; set; }
        public long? RecoveryOfficeId { get; set; }
        [ForeignKey("RecoveryOfficeId")]
        public virtual OfficeMaster fk_RecoveryOffice { get; set; }
        [DataType(DataType.Date)]
        public DateTime BillDate { get; set; }

        [Column("BillNo"), StationaryCheck, Index("IDX_CNBillMaster_BillNo", IsUnique = true),MaxLength(100)]
        public string BillNo { get; set; }
        public long? CoverNoteId { get; set; }
        [ForeignKey("CoverNoteId")]
        public virtual BillSubmission fk_CoverNote { get; set; }
        public long BillNatureId { get; set; }
        [ForeignKey("BillNatureId")]
        public virtual CNBillNature fk_BillNature { get; set; }
        [DataType(DataType.Date)]
        public DateTime? BillSubDate { get; set; }
        [MaxLength(4000)]
        public string Remarks { get; set; }

        public decimal BasicBillAmount { get; set; } = 0;
        public decimal DiscountRate { get; set; }
        public long? DiscFactorId { get; set; }
        [ForeignKey("DiscFactorId")]
        public virtual ConstantValue fk_DicFactor { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal SubtotalI { get; set; } = 0;
        [Precision(28, 7)]
        public decimal NonTaxableAmount { get; set; }//200
        public long? IGSTACId { get; set; }
        [ForeignKey("IGSTACId")]
        public virtual Ledger fk_IGSTAC { get; set; }
        public decimal IGSTRate { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;

        public long? CGSTACId { get; set; }
        [ForeignKey("CGSTACId")]
        public virtual Ledger fk_CGSTAC { get; set; }
        public decimal CGSTRate { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;

        public long? SGSTACId { get; set; }
        [ForeignKey("SGSTACId")]
        public virtual Ledger fk_SGSTAC { get; set; }
        public decimal SGSTRate { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;

        public decimal AOther1Amount { get; set; } = 0;
        public decimal AOther2Amount { get; set; } = 0;
        public decimal AOther3Amount { get; set; } = 0;
        public decimal AOther4Amount { get; set; } = 0;
        public decimal AOther5Amount { get; set; } = 0;
        public decimal AOther6Amount { get; set; } = 0;
        public decimal SubtotalII { get; set; } = 0;
        public decimal LOther1Amount { get; set; } = 0;
        public decimal LOther2Amount { get; set; } = 0;
        public decimal LOther3Amount { get; set; } = 0;
        public decimal LOther4Amount { get; set; } = 0;
        public decimal TotalBillAmount { get; set; } = 0;
        public decimal OOther1Amount { get; set; } = 0;
        public decimal OOther2Amount { get; set; } = 0;
        public decimal OOther3Amount { get; set; } = 0;
        public decimal OOther4Amount { get; set; } = 0;

        public long SalesAccountId { get; set; }
        [ForeignKey("SalesAccountId")]
        public virtual Ledger fk_SalesAc { get; set; }
        [Column("BillingPartyAcId")]
        public long BillingPartyAccountId { get; set; }
        [ForeignKey("BillingPartyAccountId")]
        public virtual Ledger fk_BillingPartyAc { get; set; }
        public long? ClientOfficeId { get; set; }
        [ForeignKey("ClientOfficeId")]
        public virtual OfficeMaster fk_ClientOffice { get; set; }
        public long? PlantLocationId { get; set; }
        [ForeignKey("PlantLocationId")]
        public virtual LedgerOffice fk_PlantLocation { get; set; }
        public long? DiscountAccountId { get; set; }
        [ForeignKey("DiscountAccountId")]
        public virtual Ledger fk_DiscountAc { get; set; }

        public long? OtherAccount1Id { get; set; }
        [ForeignKey("OtherAccount1Id")]
        public virtual Ledger fk_Other1Ac { get; set; }
        public long? OtherAccount2Id { get; set; }
        [ForeignKey("OtherAccount2Id")]
        public virtual Ledger fk_Other2Ac { get; set; }
        public long? OtherAccount3Id { get; set; }
        [ForeignKey("OtherAccount3Id")]
        public virtual Ledger fk_Other3Ac { get; set; }
        public long? OtherAccount4Id { get; set; }
        [ForeignKey("OtherAccount4Id")]
        public virtual Ledger fk_Other4Ac { get; set; }
        public long? OtherAccount5Id { get; set; }
        [ForeignKey("OtherAccount5Id")]
        public virtual Ledger fk_Other5Ac { get; set; }
        public long? OtherAccount6Id { get; set; }
        [ForeignKey("OtherAccount6Id")]
        public virtual Ledger fk_Other6Ac { get; set; }

        public MasterStatus Status { get; set; }
        [MaxLength(300)]
        public string CancelRemarks { get; set; }
        [DataType(DataType.Date)]
        public DateTime? CancelDate { get; set; }
        
        public long? ViewId { get; set; }
        public long? RefTransactionId { get; set; }
        public virtual List<CNBillLog> BillLogs { get; set; }
        public long? VDRId { get; set; }
        [ForeignKey("VDRId")]
        public virtual VoucherDetailReference fk_VDR { get; set; }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(BillNo))
            {
                yield return new ValidationResult("Bill Number cannot be blank/empty");
            }
            if (BillOfficeId == 0)
            {
                yield return new ValidationResult("Billing Office Is Required");
            }
            if (BillOfficeId>0&&RecoveryOfficeId.GetValueOrDefault() == 0)
            {
                RecoveryOfficeId = BillOfficeId;
            }
        }

        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        public bool IsTaxApplicable { get; set; }
        public string BillRef1 { get; set; }
        public string BillRef2 { get; set; }
        public string BillRef3 { get; set; }
        public string BillRef4 { get; set; }
        public string data { get; set; }

        [MaxLength(50)]
        public string TPT_RequestId { get; set; }
        public string JsonBillLogs
        {
            get => _jsonBillLogs;
            set
            {
                _jsonBillLogs = value;
            }
        }

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

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        [Column("ReasonId")]
        public long? ReasonId { get; set; }
        [ForeignKey("ReasonId")]
        public virtual GenericMaster fk_Reason { get; set; }

        [MaxLength(500)]
        public string OtherReason { get; set; }


        public int? PrintCount { get; set; }

        //public DateTime? APRLDateTime { get; set; }
        //public string APRLRemark { get; set; }
        //public long? APRLSID { get; set; }
        //public long? APRLUserId { get; set; }
        //public bool IsAutoAPRL { get; set; } = false;
        ///// <summary>
        ///// QRCode in Base64Encode
        ///// </summary>
        //public string eInvoiceQRCode { get; set; }
    }
}
