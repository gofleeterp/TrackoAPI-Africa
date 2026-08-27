using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.vw.ts
{
    public class Advance
    {
        public long Id { get; set; }
        public DateTime? AdvanceDate { get; set; }
        public long? DebitAcId { get; set; }
        public long? VDRId { get; set; }
        public long? TripLogId { get; set; }
        public long TypeId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public long? CurTypeId { get; set; }
        public decimal? CurRate { get; set; }
        public decimal SettAdvAmt { get; set; }
        public decimal DocSettAdvAmt { get; set; }
        public long? StatusId { get; set; }
        public string RefNo { get; set; }
    }

    public class FuelExpense
    {
        public long Id { get; set; }
        public long? TripLogId { get; set; }
        public long AdvanceId { get; set; }
        public decimal UsedQty { get; set; }
        public decimal UsedAmt { get; set; }
        public decimal ShortageQty { get; set; }
        public decimal ShortageAmt { get; set; }
        public bool IsBalanceZero { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string Remark { get; set; }
        public long? CurTypeId { get; set; }
        public decimal? CurRate { get; set; }
    }
    public class Expense
    {
        public long Id { get; set; }
        public decimal ClaimAmt { get; set; }
        public decimal SettledAmt { get; set; }
        public decimal FuelQty { get; set; }
        public long? AccountId { get; set; }
        
        public long? ExpNatureId { get; set; }
        public long TypeId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public long TripLogId { get; set; }
        public string Remark { get; set; }
        public long? TripAdvanceLogId { get; set; }
        public decimal Rate { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public long? CurTypeId { get; set; }
        public decimal? CurRate { get; set; }
    }

    public class TripLog
    {
        public long Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public long AddKM { get; set; }
    }
    public class TripFuelExpense
    {
        public string RefNo { get; set; }//Grid ColIndex 3
        public DateTime Date { get; set; }//Grid ColIndex 4
        public decimal TotalQty { get; set; }//Grid ColIndex 5
        public decimal Rate { get; set; }//Grid ColIndex 6
        public decimal TotalFuelAmt { get; set; }//Grid ColIndex 7
        public decimal BalanceQty { get; set; } //Grid ColIndex 8
        public decimal UsedQty { get; set; }//Grid ColIndex 9 Editable
        public decimal UsedFuelAmt { get; set; }//Grid ColIndex 10
        public decimal ShortageQty { get; set; }//Grid ColIndex 11 Editable
        public decimal ShortageFuelAmt { get; set; }//Grid ColIndex 12
        public string CrAccount { get; set; }//Grid ColIndex 13
        public long CrAccountId { get; set; }
        public string FuelType { get; set; }//Grid ColIndex 14
        public long FuelTypeId { get; set; }
        public string Driver { get; set; }//Grid ColIndex 15
        public string Description { get; set; }//Grid ColIndex 16 Editable
        public long Id { get; set; } = 0;//Grid ColIndex 17
        public long TriplogId { get; set; }//Grid ColIndex 18
        public long? SettlementId { get; set; }//Grid ColIndex 19
        public long? TypeId { get; set; }//Grid ColIndex 20
        public bool IsDeletedId { get; set; } = false; //Grid ColIndex 21
        public long AdavnceTypeId { get; set; } //Grid ColIndex 22
        public long? AdvanceId { get; set; } //Grid ColIndex 23
        public long? CurTypeId { get; set; }
        public decimal? CurRate { get; set; }
    }
}
