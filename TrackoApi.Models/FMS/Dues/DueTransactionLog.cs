using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tDueTransactionLog")]
    public class DueTransactionLog: AuditableEntity
    {
        [ForeignKey("fk_Office")]
        public long OfficeId { get; set; }
        public OfficeMaster fk_Office { get; set; }
        [ForeignKey("fk_DueType")]
        public long DueTypeId { get; set; }
        public DueMaster fk_DueType { get; set; }
        [Column("PaidDate"), Required]
        public DateTime PaidDate { get; set; }
        [MaxLength(100), StationaryCheck]
        public string VoucherNo { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; }
        [ForeignKey("DueAccountId")]
        public Ledger fk_DueAccount { get; set; }
        public long DueAccountId { get; set; }
        [ForeignKey("PayableAccountId")]
        public Ledger fk_PayableAccount { get; set; }
        public long PayableAccountId { get; set; }
        [ForeignKey("OthPayableAccountId")]
        public Ledger fk_OthPayableAccount { get; set; }
        public long? OthPayableAccountId { get; set; }
        #region gst details
            [ForeignKey("IGSTAccountId")]
            public Ledger fk_IGSTAccount { get; set; }
            public long? IGSTAccountId { get; set; }
            [Column("IGSTPAmount")]
            public decimal IGSTPAmount { get; set; } = 0;
            [Column("IGSTPAmountP")]
            public decimal IGSTPAmountP { get; set; } = 0;
            [Column("IGSTTPAmount")]
            public decimal IGSTTPAmount { get; set; } = 0;
             [Column("IGSTTPAmountP")]
            public decimal IGSTTPAmountP { get; set; } = 0;

            [ForeignKey("CGSTAccountId")]
            public Ledger fk_CGSTAccount { get; set; }
            public long? CGSTAccountId { get; set; }
            [Column("CGSTPAmount")]        
            public decimal CGSTPAmount { get; set; } = 0;
            [Column("CGSTPAmountP")]
            public decimal CGSTPAmountP { get; set; } = 0;

            [Column("CGSTTPAmount")]
            public decimal CGSTTPAmount { get; set; } = 0;
            [Column("CGSTTPAmountP")]
            public decimal CGSTTPAmountP { get; set; } = 0;

            [ForeignKey("SGSTAccountId")]
            public Ledger fk_SGSTAccount { get; set; }
            public long? SGSTAccountId { get; set; }     
            [Column("SGSTPAmount")]
            public decimal SGSTPAmount { get; set; } = 0;
            [Column("SGSTPAmountP")]
            public decimal SGSTPAmountP { get; set; } = 0;
            [Column("SGSTTPAmount")]
            public decimal SGSTTPAmount { get; set; } = 0;
            [Column("SGSTTPAmountP")]
            public decimal SGSTTPAmountP { get; set; } = 0;

        #endregion
        [ForeignKey("OtherAccountId")]
        public Ledger fk_OtherAccount { get; set; }
        public long? OtherAccountId { get; set; }

        [Column("VehicleId"), Required, ForeignKey("fk_Vehicle")]
        public long VehicleId { get; set; }
        public VehicleMaster fk_Vehicle { get; set; }
        [Column("RefNo1"),MaxLength(100), Index("IX_DueTransactionLog_RefNo1", IsUnique = true)]
        public string RefNo1 { get; set; }
        [Column("RefNo2"), MaxLength(100)]
        public string RefNo2 { get; set; }
        [Column("StartDate"), Required]
        public DateTime StartDate { get; set; }

        [Column("ExpiryDate"), Required]
        public DateTime ExpiryDate { get; set; }

        [Column("MiscCharge")]
        public decimal MiscCharge { get; set; }

        [Column("DueAmount")]
        public decimal DueAmount { get; set; }

        [Column("OtherAmount")]
        public decimal OtherAmount { get; set; } = 0;
        [ForeignKey("fk_Owner")]
        public long? OwnerId { get; set; }
        public Ledger fk_Owner { get; set; }
        [Column("VoucherId"), Required, ForeignKey("fk_Voucher")]
        public long VoucherId { get; set; }
        public virtual Voucher fk_Voucher { get; set; }
        public DueInsuranceLog fk_InsuranceLog { get; set; }
        public bool IsBulkEntry { get; set; } = false;
        public decimal PrePaidTax { get; set; } = 0;
        public bool IsPrePaidTaxBooked { get; set; } = false;
        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public virtual DueTransactionLog fk_NextLog { get; set; }
        public long? PreviousLogId { get; set; }
        [ForeignKey("PreviousLogId")]
        public virtual DueTransactionLog fk_PreviousLog { get; set; }
        public long? ViewId { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
    }
}
