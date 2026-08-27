using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mExpenseMaster")]
    public class ExpenseMaster : AuditableEntity
    {
        [Column("Name"), Required, Index("IDX_mExpenseMaster_Name", IsUnique = true), MaxLength(30)]
        public string Name { get; set; }

        [Column("Abbr"), Required, Index("IDX_mExpenseMaster_Abbr", IsUnique = true), MaxLength(30)]
        public string Abbr { get; set; }

        [Column("ExpenseCategoryId"), ForeignKey("fk_ExpenseCategory")]
        public long? ExpenseCategoryId { get; set; }
        public virtual GenericMaster fk_ExpenseCategory { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

        [ForeignKey("fk_Ledger")]
        public long? LedgerId { get; set; }
        public virtual Ledger fk_Ledger { get; set; }
        public long? NatureId { get; set; }
        [ForeignKey("NatureId")]
        public virtual ConstantValue fk_Nature { get; set; }

        public bool IsTax { get; set; }
        
        public long? AutoExpId { get; set; }
        [ForeignKey("AutoExpId")]
        public virtual ExpenseMaster fk_AutoExp { get; set; }
        [MaxLength(200)]
        public string AutoExpRuleKey { get; set; }
        /// <summary>
        /// Type: Trip Expense(s) or Hire Trip Expense(s)
        /// </summary>
        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }

        public decimal MaxExpense { get; set; }

        public bool Flag1 { get; set; } = false;    
    }
}
