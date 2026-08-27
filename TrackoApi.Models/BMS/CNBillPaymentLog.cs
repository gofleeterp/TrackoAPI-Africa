using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Models.BMS
{
    [Table("tCNBillPaymentLog")]
    public class CNBillPaymentLog : AuditableEntity {
        private long? _vdrId;
        public long PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public virtual CNBillPayment fk_Payment { get; set; }
        public long AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Ledger fk_Account { get; set; }
        
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        /// <summary>
        /// Type: Constant TypeId=111
        /// </summary>
        
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
        public long? BillId { get; set; }
        [ForeignKey("BillId")]
        public virtual CNBill fk_Bill { get; set; }
        public long? BillLogId { get; set; }
        [ForeignKey("BillLogId")]
        public virtual CNBillLog fk_BillLog { get; set; }

        public long? BillLogArchiveId { get; set; }
        [ForeignKey("BillLogArchiveId")]
        public virtual CNBillLog fk_BillLogArchive { get; set; }
        public long? CNId { get; set; }
        [ForeignKey("CNId")]
        public virtual CNMaster fk_CN { get; set; }
        [Column("TripLogId")]
        public long? TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }
        public decimal TDSRate { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal Amount { get; set; }
        [MaxLength(5000)]
        public string Remark { get; set; }

        public long? OnAccountRefId { get; set; }
        [ForeignKey("OnAccountRefId")]
        public virtual CNBillPaymentLog fk_OnAccountRef { get; set; }
        [Column("OnAcAdjustedAmt")]
        public decimal OnAccountAdjustedAmount { get; set; }

        public virtual List<CNBillPaymentLog> OnAcSettlements { get; set; }
        [Column("OnAccBalAmt")]
        public decimal OnAccountBalanceAmount { get; set; }

        public long? VDRId
        {
            get { return _vdrId; }
            set
            {
                Debug.Assert(value != 0);
                if (value == 0) value = null;
                _vdrId = value;
                
            }
        }

        [ForeignKey("VDRId")]
        public virtual VoucherDetailReference fk_VDR { get; set; }
        
        public long? DeductionTypeId { get; set; }
        [ForeignKey("DeductionTypeId")]
        public virtual PaymentDeductionType fK_DeductionType { get; set; }
        public long? TripAdvanceId { get; set; }
        [ForeignKey("TripAdvanceId")]
        public virtual TripAdvanceLog fk_TripAdvance { get; set; }


    }
    
}