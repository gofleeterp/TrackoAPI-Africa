using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Models.BMS
{
    [Table("tCNBillPayment")]
    public class CNBillPayment : AuditableEntity
    {
        public CNBillPayment()
        {
            this.PaymentLogs=new List<CNBillPaymentLog>();
            this.BulkLog  = new List<vwBillPaymentLog>();
        }
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office {get;set;}
        [MaxLength(300), StationaryCheck]
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }
        [MaxLength(300)]
        public string AdviceNo { get; set; }
        public DateTime? AdviceDate { get; set; }
        [MaxLength(300)]
        public string ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public long ClientAcId { get; set; }
        [ForeignKey("ClientAcId")]
        public virtual Ledger fk_ClientAc { get; set; }
        public long? BankCashAccountId { get; set; }
        [ForeignKey("BankCashAccountId")]
        public virtual Ledger fk_BankCashAccount { get; set; }
        
        public long? TDSLedgerAcId { get; set; }
        [ForeignKey("TDSLedgerAcId")]
        public virtual Ledger fk_TDSLedgerAc { get; set; }
        public long? Other1AcId { get; set; }
        [ForeignKey("Other1AcId")]
        public virtual Ledger fk_Other1Ac { get; set; }
        public long? Other2AcId { get; set; }
        [ForeignKey("Other2AcId")]
        public virtual Ledger fk_Other2Ac { get; set; }
        [MaxLength(300)]
        public string DraweeBank { get; set; }
        [MaxLength(300)]
        public string DraweeBranch { get; set; }
        /// <summary>
        /// Payment Mode: Constant TypeId =110
        /// </summary>
        public long? PaymentModeId { get; set; }
        [ForeignKey("PaymentModeId")]
        public virtual ConstantValue fk_PaymentMode { get; set; }
        [MaxLength(10000)]
        public string Remark { get; set; }
        [MaxLength(10000)]
        public string AcNarration { get; set; }
        public decimal BankCashAmount { get; set; } = 0;
        public decimal TDSAmount { get; set; } = 0;
        public decimal OtherAmount { get; set; } = 0;
        public decimal AdviceAmount { get; set; } = 0;
        [Column("DeductionAmt")]
        public decimal DeductionAmount { get; set; }
        [Column("BillSettledAmt")]

        public decimal BillSettledAmount { get; set; }
        [Column("OnAcAmt")]
        public decimal OnAccountAmount { get; set; } = 0;

        [Column("OnAcAdjustedAmt")]
        public decimal OnAccountAdjustedAmount { get; set; } = 0;
        [Column("CNAdvanceAmt")]
        public decimal CNAdvanceAmount { get; set; } = 0;

        public virtual List<CNBillPaymentLog> PaymentLogs { get; set; }
        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        [Column("VoucherTypeId"),  ForeignKey("FK_VoucherType")]
        public long? VoucherTypeId { get; set; }
        public virtual VoucherType FK_VoucherType { get; set; }
        public long? ViewId { get; set; }

        public virtual List<vwBillPaymentLog> BulkLog { get; set; } = new List<vwBillPaymentLog>();
        public bool GenerateVoucherOnServer { get; set; } = false;

        [Column("ReasonId")]
        public long? ReasonId { get; set; }
        [ForeignKey("ReasonId")]
        public virtual GenericMaster fk_Reason { get; set; }

        [MaxLength(500)]
        public string OtherReason { get; set; }
    }

}