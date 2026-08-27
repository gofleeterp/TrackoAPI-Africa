using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS.Tyres;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mTyreMaster")]
    public class TyreMaster : AuditableEntity,IValidatableObject
    {

        [Column("TyreNo"),Required,MaxLength(100), Index("IDX_mTyreMaster_TyreNo", IsUnique = true)]
        public string TyreNo { get; set; }

        [Column("BrandId"),ForeignKey("fk_Brand")]
        public long BrandId { get; set; }
        public BrandMaster fk_Brand { get; set; }

        [Column("ProdMonth")]
        public DateTime ProdMonth { get; set; }

        [Column("OpeningKm")]
        public long OpeningKm { get; set; }

        [Column("OpeningMonth")]
        public long OpeningMonth { get; set; }

        [Column("IsAnalysis")]
        public bool IsAnalysis { get; set; }
        
        [Column("PurchaseVoucherId")]
        public long? PurchaseVoucherId { get; set; }
        [ForeignKey("PurchaseVoucherId")]
        public virtual Voucher fk_PurchaseVoucher { get; set; }
        public long? PurchaseLogId { get; set; }
        [ForeignKey("PurchaseLogId")]
        public virtual TyreLog fk_PurchaseTyreLog { get; set; }
        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }
        #region Tyre CurrentStatus
        [Column("Status_VoucherDate")]
        public DateTime S_VoucherDate { get; set; }
        [Column("Status_VoucherId")]
        public long? S_VoucherId { get; set; }
        [ForeignKey("S_VoucherId")]
        public virtual Voucher fk_S_Voucher { get; set; }
        /// <summary>
        /// Gets or sets the tyre log identifier.
        /// </summary>
        /// <value>The tyre log identifier.</value>
        
        [Column("Status_TyreLogId")]
        public long? S_TyreLogId { get; set; }
        [ForeignKey("S_TyreLogId")]
        public virtual TyreLog  fk_S_TyreLog { get; set; }
        [Column("Status_VoucherTypeId")]
        public long S_VoucherTypeId { get; set; }
        [ForeignKey("S_VoucherTypeId")]
        public virtual VoucherType fk_S_VoucherType { get; set; }
        [Column("Status_DebitAccountId")]
        public long S_DebitAccountId { get; set; }
        [ForeignKey("S_DebitAccountId")]
        public virtual Ledger fk_S_DebitAccount { get; set; }
        [Column("Status_CreditAccountId")]
        public long S_CreditAccountId { get; set; }
        [ForeignKey("S_CreditAccountId")]
        public virtual Ledger fk_S_OtherAccount { get; set; }
        [Column("Status_StatusId")]
        public long S_StatusId { get; set; }
        [ForeignKey("S_StatusId")]
        public virtual ConstantValue fk_S_Status { get; set; }
        [Column("Status_Life")]
        public int S_Life { get; set; }

        public virtual List<TyreMillageLog> TyreMillageLogs { get; set; }
        #endregion
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BrandId == 0)
            {
                yield return new ValidationResult("Brand is required",new []{ "BrandId" });
            }
            if (ProdMonth.Date > DateTime.Today)
            {
                yield return new ValidationResult($"Production Month has invalid value {ProdMonth.Date:d}", new[] { "ProdMonth" });
            }
        }
    }
    
}