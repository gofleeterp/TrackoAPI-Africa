using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.AMS
{
    [Table("tVoucherVDR")]
    public class VoucherDetailReference : AuditableEntity, IValidatableObject
    {
        public bool IsCCRequired { get; set; } = true;
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        public decimal Amount_FX { get; set; } = 0;
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;
        [Precision(28, 4)]
        public decimal OldCurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        [Column("VDId"),Required,ForeignKey("fk_VoucherDetail")]
        public long VoucherDetailId { get; set; }
        public virtual VoucherDetail fk_VoucherDetail { get; set; }
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
        [Column("ReferenceNo"),MaxLength(200)]
        public string ReferenceNo { get; set; }
        [Column("VDRTypeID"),Required,ForeignKey("fk_VDRType")]
        public long VDRTypeId { get; set; }
        public virtual ConstantValue fk_VDRType { get; set; }
        [Column("RefID"),ForeignKey("fk_ParentReference")]
        public long? RefId { get; set; }
        public virtual VoucherDetailReference fk_ParentReference { get; set; }
        [Column("OriginalRefId"), ForeignKey("fk_OriginalReference")]
        public long? OriginalRefId { get; set; }
        public virtual VoucherDetailReference fk_OriginalReference { get; set; }
        
        [Column("VDRAmount")]
        public decimal Amount { get; set; }

        [Column("VDRRefAmount")]
        public decimal VDRRefAmount { get; set; }

        [Column("VDRAmount_MNC")]
        public decimal Amount_MNC { get; set; }
        public virtual List<VoucherDetailReference> AgainstReferences { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// it should be transaction Id incase the VRD are generated from fleet or Booking
        /// </summary>
        /// <value>The transaction identifier.</value>
        public long? TransactionId { get; set; }
        [MaxLength(255)]
        public string BatchId { get; set; }

        public long? AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Ledger fk_Account { get; set; }
        public long? ActualVDId { get; set; }
        //[ForeignKey("ActualVDId")]
        //public virtual VoucherDetail fk_ActualVD { get; set; }
        public VoucherDetailReference Clone()
        {
            return (VoucherDetailReference)this.MemberwiseClone();
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.VDRRefAmount == 0) {
                this.VDRRefAmount = this.Amount;
            }
            if (1==2)
            {
                yield return new ValidationResult("No Msg", new[] { "Amount" });
            }
        }
    }
    public class VDRBalance
    {
        public long VDRId { get; set; }
        public long VDId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string ReferenceNo { get; set; }
        public long VDRTypeID { get; set; }
        public long? OriginalRefId { get; set; }
        public decimal VDRAmount { get; set; }
        public long? TransactionId { get; set; }
        public long? AccountId { get; set; }
        public long CSID { get; set; }
        public DateTime CDOE { get; set; }
        public long CreditDays { get; set; }
        public long? CreditNatureId { get; set; }
        public decimal PreviousPaid { get; set; }
        public decimal Balance { get; set; }
        public string RefType { get; set; }
        public string AccountName { get; set; }
        public long VoucherId { get; set; }
    }
}
