using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;

namespace TrackoAPI.ViewModels.FMS.Dues
{
    public class vwDueVoucher
    {
        public vwDueVoucher()
        {
            DueLogs=new List<vwDueTransactionLog>();
        }
        [Key]
        public long Id { get; set; }
        public long OfficeId { get; set; }
        public long DueTypeId { get; set; }
        public DateTime PaidDate { get; set; }
        [MaxLength(100)]
        public string DocumentNo { get; set; }
        [MaxLength(200)]
        public string Narration { get; set; }
        public long DueAccountId { get; set; }
        public string DueAccountName { get; set; }
        public decimal DueAmount { get; set; } = 0;
        public decimal MiscCharg { get; set; } = 0;
        public long PayableAccountId { get; set; }
        public string PayableAccountName { get; set; }
        public decimal PayableAmount { get; set; } = 0;
        public long? OthPayableAccountId { get; set; }
        public string OthPayableAccountName { get; set; }
        public decimal OthPayableAmount { get; set; } = 0;
        public long? OtherAccountId { get; set; }
        public string OtherAccountName { get; set; }
        public decimal OtherAmount { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;
        public int PaymentMode { get; set; }
        public string ChequeNo { get; set; }
        public Nullable<DateTime> ChequeDate { get; set; }
        public List<vwDueTransactionLog> DueLogs { get; set; }

        #region  gst details
        public long? IGSTAccountId { get; set; }
        public string IGSTAccountName { get; set; }
        public decimal IGSTAmount { get; set; } = 0;
        public long? CGSTAccountId { get; set; }
        public string CGSTAccountName { get; set; }
        public decimal CGSTAmount { get; set; } = 0;
        public long? SGSTAccountId { get; set; }
        public string SGSTAccountName { get; set; }
        public decimal SGSTAmount { get; set; } = 0;
        #endregion
        public string OfficeName { get; set; }

        public bool IsLocked { get; set; }
        public long? PageId { get; set; }
        public long? ViewId { get; set; }
        public long? CurTypeId { get; set; }

        public long? ConstCurTypeId { get; set; }
        
        public decimal CurRate { get; set; } = 0;
    }
    [ComplexType, EdmComplexType]
    public class vwDueTransactionLog
    {
        public long Id { get; set; }
        [Required]
        public long VehicleId { get; set; }
        [MaxLength(100)]
        public string RefNo1 { get; set; }
        [ MaxLength(20)]
        public string RefNo2 { get; set; }
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public decimal DueAmount { get; set; }
        public decimal MiscCharg { get; set; } = 0;
        public decimal OtherAmount { get; set; }
        public long? OwnerId { get; set; }
        public string OwnerName { get; set; }
        [MaxLength(20)]
        public string Remark { get; set; }
        public vwDueInsuranceLog InsuranceLog { get; set; }

        public long IGSTAccountId { get; set; }
        public string IGSTAccountName { get; set; }
        public long CGSTAccountId { get; set; }
        public string CGSTAccountName { get; set; }
        public long SGSTAccountId { get; set; }
        public string SGSTAccountName { get; set; }
        public decimal IGSTPAmount { get; set; } = 0;
        public decimal CGSTPAmount { get; set; } = 0;
        public decimal SGSTPAmount { get; set; } = 0;
        
        public decimal IGSTPAmountP { get; set; } = 0;
        public decimal CGSTPAmountP { get; set; } = 0;
        public decimal SGSTPAmountP { get; set; } = 0;


        public decimal IGSTTPAmount { get; set; } = 0;
        public decimal CGSTTPAmount { get; set; } = 0;
        public decimal SGSTTPAmount { get; set; } = 0;

        public decimal IGSTTPAmountP { get; set; } = 0;
        public decimal CGSTTPAmountP { get; set; } = 0;
        public decimal SGSTTPAmountP { get; set; } = 0;
        public string VehicleNo { get; set; }
        public bool IsDeleted { get; set; }

        public long DueTypeId { get; set; }
        public string DueTypeName { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        
        public decimal CurRate { get; set; } = 0;
    }
}
