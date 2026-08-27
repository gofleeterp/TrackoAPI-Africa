using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackoApi.Models.AMS
{
    [Table("mFY")]
    public class FinancialYear:Base.AuditableEntity
    {
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Abbreviation { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime? LockUpToDate { get; set; }
        public bool IsLocked { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
