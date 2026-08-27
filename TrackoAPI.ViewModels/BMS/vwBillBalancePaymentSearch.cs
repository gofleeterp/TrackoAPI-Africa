using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.BMS
{
    public class vwBillBalancePaymentSearch
    {
        public long Id { get; set; }
        public string BillNo { get; set; }
        public string CNNo { get; set; }
        public decimal BalanceAmount { get; set; }
    }
    [EdmComplexType]
    public class vwBillPaymentLog
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long OfficeId { get; set; }
        public long TypeId { get; set; }
        public long? BillId { get; set; }
        public long? BillLogId { get; set; }
        public long? CNId { get; set; }
        public string CNNo { get; set; }
        public long? TripLogId { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public long? OnAccountRefId { get; set; }
        public decimal OnAccountAdjustedAmount { get; set; }
        public decimal OnAccountBalanceAmount { get; set; }

        public long? VDRId { get; set; }

        public long? DeductionTypeId { get; set; }
        public long? TripAdvanceId { get; set; }
        public decimal DriverAdvAmt { get; set; }
        public long? TLId { get; set; }
        public long? DriverId { get; set; }
        public long? VehicleId { get; set; }
        public long? DriverAdvDrAccountId { get; set; }
        public decimal TDSRate { get; set; }
        public decimal TDSAmount { get; set; }
    }
}
