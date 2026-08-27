using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS.Loan
{
    [Table("tLoanLog")]
   public class LoanLog : AuditableEntity
    {
        public long LoanId { get; set; }
        [ForeignKey("LoanId")]
        public virtual Loan fk_Loan { get; set; }
        public DateTime? LogDate { get; set; }
        public string LoanLogNo { get; set; }

        public decimal PrincipalAmount { get; set; } = 0;
        public decimal InterestAmount { get; set; } = 0;
        //public decimal EMI { get; set; } = 0;

        public decimal InstallmentAmount { get; set; } = 0;
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        public long? ParentLogId { get; set; }
        [ForeignKey("ParentLogId")]
        public virtual LoanLog fk_ParentLogs { get; set; }

        public string Remarks { get; set; }
        public long? VDRId { get; set; }
        [ForeignKey("VDRId")]
        public virtual VoucherDetailReference fk_VDR { get; set; }


        public long? LoanVoucherId { get; set; }
        [ForeignKey("LoanVoucherId")]
        public virtual Voucher fk_LoanVoucher { get; set; }

        public long? RepVoucherId { get; set; }
        [ForeignKey("RepVoucherId")]
        public virtual Voucher fk_RepVoucher { get; set; }

        [MaxLength(150)]
        public string RepayVoucherNo { get; set; }

        public DateTime? RepDate { get; set; }
        public decimal TDSRate { get; set; } = 0;
        public decimal TDSAmount { get; set; } = 0;
        public decimal RepAmount { get; set; } = 0;

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
