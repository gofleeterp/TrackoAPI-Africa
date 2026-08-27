using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tHSAdvance")]
    public class HSAdvance : AuditableEntity
    {
        [Column("AdvTypeId")] //Advance/Adjustment//Balance Payment//On Account
        public long? AdvTypeId { get; set; }
        [ForeignKey("AdvTypeId")]
        public virtual ConstantValue fk_AdvanceType { get; set; }
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        [Column("VoucherTypeId")] //Advance/Adjustment
        public long VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }

        [Column("VoucherId")] //Advance/Adjustment
        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        public long? HireSlipId { get; set; }
        [ForeignKey("HireSlipId")]
        public virtual VehicleMovementLog fk_HireSlip { get; set; }

        [Column("RefAdvId")]
        public long? RefAdvId { get; set; } // incase of settlement
        [ForeignKey("RefAdvId")]
        public virtual HSAdvance fk_RefHSAdvance { get; set; }
        public virtual List<HSAdvance> Settlements { get; set; }

        [Column("AdvDate"), Required]
        public DateTime AdvDate { get; set; }

        [Column("CrAccountId")]
        public long CrAccountId { get; set; }
        [ForeignKey("CrAccountId")]
        public virtual Ledger fk_CreditAccount { get; set; }
        [Column("DrAccountId")]
        public long DrAccountId { get; set; }
        [ForeignKey("DrAccountId")]
        public virtual Ledger fk_DreditAccount { get; set; }
        [MaxLength(100)]
        public string RefNo { get; set; }
        public decimal FuelQty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; } = 0;

        [Column("PaymentModeId")]
        public long? PaymentModeId { get; set; }
        [ForeignKey("PaymentModeId")]
        public virtual ConstantValue fk_PaymentMode { get; set; }
        [MaxLength(20)]
        public string ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        [MaxLength(100)]
        public string BankName { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
        public long? ViewId { get; set; }
        public long? VDRId { get; set; }
        [ForeignKey("VDRId")]
        public virtual VoucherDetailReference fk_VDR { get; set; }

    }
}
