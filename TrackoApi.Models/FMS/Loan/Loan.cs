using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

namespace TrackoApi.Models.FMS.Loan
{
    [Table("tLoan")]
   public class Loan :AuditableEntity
    {
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public DateTime? LoanDate { get; set; }
        [StationaryCheck]
        public string LoanNo { get; set; }

        public long? CreditAcId { get; set; }
        [ForeignKey("CreditAcId")]
        public virtual Ledger fk_CreditAc { get; set; }

        public long? DebitAcId { get; set; }
        [ForeignKey("DebitAcId")]
        public virtual Ledger fk_DebitAc { get; set; }

        public long? InterestAcId { get; set; }
        [ForeignKey("InterestAcId")]
        public virtual Ledger fk_InterestAc { get; set; }

        public long? TDSAcId { get; set; }
        [ForeignKey("TDSAcId")]
        public virtual Ledger fk_TDSAc { get; set; }

        [Column("ChequeId")]
        public long? ChequeId { get; set; }

        [Column("ChequeNo"), MaxLength(50)]
        public string ChequeNo { get; set; }

        [Column("ChequeDate")]
        public DateTime? ChequeDate { get; set; }

        [Column("ChequeBank"), MaxLength(50)]
        public string ChequeBank { get; set; }


        public decimal InstallmentAmount { get; set; } = 0;
        public decimal PrincipalAmount { get; set; } = 0;
        public decimal InterestAmount { get; set; } = 0;
        public decimal TDSAmount { get; set; } = 0;
        //public decimal IRR { get; set; } = 0;
        public decimal RateOfInterest { get; set; } = 0;
        public long? LoanTenure { get; set; } = 0;
        public long? NoofEMI { get; set; } = 0;
        /// <summary>
        /// type:Schedule/Repayment
        /// </summary>
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
        /// <summary>
        /// period: Monthly/Quaterly/Half Yearly/Yearly
        /// </summary>
        public long? PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual ConstantValue fk_Period { get; set; }
        public long? NoofInstallment { get; set; }
        public DateTime? FirstInstallmentDate { get; set; }
        public DateTime? PrecloserDate { get; set; }
        //public decimal EMI { get; set; } = 0;
        public string Remarks { get; set; }

        public virtual List<LoanLog> Logs { get; set; }
        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }
    }
}
