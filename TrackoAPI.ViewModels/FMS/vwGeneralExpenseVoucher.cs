using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackoAPI.ViewModels.FMS
{
    public class vwGeneralExpenseVoucher
    {
        [Key]
        public long Id { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
        public long? PageId { get; set; }
        public string DocumentNo { get; set; }
        public long CrAccountId { get; set; }
        public string CrAccountName { get; set; }
        public decimal BasicAmount { get; set; }
        public long DrAccountId { get; set; }
        public string DrAccountName { get; set; }
        public DateTime DocumentDate { get; set; }
        public long OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string Remark { get; set; }
        public decimal NetAmount { get; set; }
        public List<vwGeneralExpenseLog> GeneralExpenseLogs { get; set; }
        public bool IsLocked { get; set; }
        public long? ViewId { get; set; }
        public long? VoucherTypeId { get; set; }
        public string BatchId { get; set; }
        public long? IGSTAccountId { get; set; }
        public long? CGSTAccountId { get; set; }
        public long? SGSTAccountId { get; set; }
        public decimal IGSTAmount { get; set; }
        public decimal CGSTAmount { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal SGSTRate { get; set; }
    }
    public class vwGeneralExpenseLog
    {
        public long? CNId { get; set; }
        public long ExpenseId { get; set; }
        public string VoucherNo { get; set; }
        public string PaidIn { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public DateTime ExpenseDate { get; set; }
        public long OfficeId { get; set; }
        public string OfficeName { get; set; }
        public long CreditAccountId { get; set; }
        public string CreditAccount { get; set; }
        public long DebitAccountId { get; set; }
        public string DebitAccountName { get; set; }
        public decimal Amount { get; set; } = 0;
        public long? DriverId { get; set; } = 0;
        public string DriverName { get; set; }
        public string Remark { get; set; }
        public string ReferenceNo { get; set; }
        public long? VoucherId { get; set; }
        public long? ViewId { get; set; }
        public long? PaidInId { get; set; }
        public string Ref1 { get; set; }
        public long? ExpenseNatureId { get; set; }
        public string ExpenseNature { get; set; }
        public decimal Amount1 { get; set; }
        public decimal Amount2 { get; set; }
        public long? SettlementId { get; set; }
        public long? TripLogId { get; set; }

        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
    }
}
