using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mBatteryMaster")]
    public class BatteryMaster : AuditableEntity,IValidatableObject
    {

        [Column("SerialNo"),Required,MaxLength(100), Index("IDX_mBatteryMaster_SerialNo", IsUnique = true)]
        public string BatterySerialNo { get; set; }

        public int? NoofCells { get; set; }

        [Column("BrandId")]
        public long BrandId { get; set; }
        [ForeignKey("fk_Brand")]
        public virtual BatteryBrand fk_Brand { get; set; }

        [Column("OpeningAge")]
        public long OpeningAge { get; set; }

        [Column("IsAnalysis")]
        public bool IsAnalysis { get; set; }
        
        [Column("PurchaseExtraInfoId")]
        public long? PurchaseExtraInfoId { get; set; }
        [ForeignKey("PurchaseExtraInfoId")]
        public virtual BatteryLogExtraInfo fk_PurchaseExtraInfo { get; set; }
        public long? PurchaseLogId { get; set; }
        [ForeignKey("PurchaseLogId")]
        public virtual BatteryLog fk_PurchaseBatteryLog { get; set; }

        #region Battery CurrentStatus
        [Column("Status_DocDate")]
        public DateTime S_DocDate { get; set; }

        [Column("Status_ExtraInfoId")]
        public long? S_ExtraInfoId { get; set; }
        [ForeignKey("S_ExtraInfoId")]
        public virtual BatteryLogExtraInfo fk_S_ExtraInfo { get; set; }
        /// <summary>
        /// Gets or sets the Battery log identifier.
        /// </summary>
        /// <value>The Battery log identifier.</value>
        
        [Column("Status_BatteryLogId")]
        public long? S_BatteryLogId { get; set; }
        [ForeignKey("S_BatteryLogId")]
        public virtual BatteryLog  fk_S_BatteryLog { get; set; }
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
        #endregion
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BrandId == 0)
            {
                yield return new ValidationResult("Brand is required",new []{ "BrandId" });
            }
        }
    }
    
}