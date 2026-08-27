using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.AMS
{
    [Table("tVoucherVD")]
    public class VoucherDetail : AuditableEntity,IValidatableObject
    {
        public VoucherDetail()
        {
            VoucherDetailReferences = new List<VoucherDetailReference>();
            Amount = 0;
        }
        public bool IsCCRequired { get; set; } = true;
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;        
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        [Column("VoucherId"), Required, ForeignKey("Voucher")]
        public long VoucherId { get; set; }

        public virtual Voucher Voucher { get; set; }

        [Column("OfficeId"), Required]
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("AccountId"), Required,ForeignKey("fk_Account")]
        public long AccountId { get; set; }

        public virtual Ledger fk_Account { get; set; }

        [Column("OrderId"), Required]
        public long OrderId { get; set; }

        [Column("VDAmount")]
        public decimal Amount { get; set; }

        [Column("VDAmount_MNC")]
        public decimal Amount_MNC { get; set; } = 0;
        public decimal Amount_FX { get; set; } = 0;

        [Column("ChequeId")]
        public long? ChequeId { get; set; }

        [Column("ChequeNo"), MaxLength(50)]
        public string ChequeNo { get; set; }

        [Column("ChequeDate")]
        public DateTime? ChequeDate { get; set; }

        [Column("ChequeBank"), MaxLength(50)]
        public string ChequeBank { get; set; }
        public DateTime? BankRecoDate { get; set; }

        [Column("Narration"), MaxLength(4000)]
        public string Narration { get; set; }
        [Column("Particular"), MaxLength(2000)]
        public string Particular { get; set; }

        public virtual List<VoucherDetailReference> VoucherDetailReferences { get; set; }
        public string JsonVDRS { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var vdrtot = VoucherDetailReferences.Where(x => x.ObjectState != ObjectState.Deleted)
                    .Sum(x => x.Amount_MNC);
            var fxtot = VoucherDetailReferences.Where(x => x.ObjectState != ObjectState.Deleted).Sum(c=>c.Amount_FX);
            if (VoucherDetailReferences != null && VoucherDetailReferences.Count > 0 && Amount_MNC != vdrtot)
            {
                Amount_MNC = vdrtot;
            }
            if (fxtot!=0)
            {
                Amount_MNC = vdrtot;
                Amount_FX = fxtot;
            }
            else
            {
                if (VoucherDetailReferences != null && VoucherDetailReferences.Count > 0 && Amount_MNC != vdrtot)
                {
                    yield return new ValidationResult($"Sum of VoucherReference's Amount={vdrtot} should be equal to VDAmount={Amount_MNC}", new[] { "Amount" });
                }
            }
            //CurRate = ((ConstCurTypeId == CurTypeId) || CurRate <= 0) ? 1 : CurRate;
            //if (CurTypeId != ConstCurTypeId & CurTypeId.GetValueOrDefault() > 0 && CurRate <= 0)
            //{
            //    throw new BusinessException(ErrorCode.CUR100, "VD-Model: Currency Rate need to be defined!!");
            //}
            //this.Amount_MNC = this.Amount * (1 * CurRate);
            //this.Amount1_MNC = this.Amount1 * (1 * CurRate);
        }
        [MaxLength(255)]
        public string BatchId { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount1 { get; set; }

        [Column("Amount1_MNC")]
        public decimal Amount1_MNC { get; set; } = 0;

        public long? TaxTypeId { get; set; }
        [ForeignKey("TaxTypeId")]
        public virtual TaxServiceType fk_TaxType { get; set; }
        [Column("Constant1Id")]
        public long? Constant1Id { get; set; }
        [ForeignKey("Constant1Id")]
        public virtual ConstantValue fk_Constant1Value { get; set; }
        [Column("Account1Id")]
        public long? Account1Id { get; set; }
        [ForeignKey("Account1Id")]
        public virtual Ledger fk_Account1 { get; set; }
        public VoucherDetail Clone()
        {
            return (VoucherDetail)this.MemberwiseClone();
        }
    }
}