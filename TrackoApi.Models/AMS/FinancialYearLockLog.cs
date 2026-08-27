using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackoApi.Models.AMS
{
    
    [Table("tFYLedgerLockLog")]
    public class FinancialYearLedgerLockLog : Base.AuditableEntity
    {
        [Index("IDX_Unique_FYLedgerLock",IsUnique =true,Order =1)]
        public long FinancialYearId { get; set; }
        [ForeignKey("FinancialYearId")]
        public virtual FinancialYear fk_FinancialYear { get; set; }
        [Index("IDX_Unique_FYLedgerLock", IsUnique = true, Order = 2)]
        public long LedgerId { get; set; }
        [ForeignKey("LedgerId")]
        public virtual Ledger fk_Ledger { get; set; }
        public DateTime LockedDate { get; set; }
    }
}