using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.AMS
{
    [Table("mLedgerRole")]
    public class LedgerRole : AuditableEntity
    {
        [ForeignKey("fk_Ledger"), Required,Index("IDX_LedgerRole_UniqueKey",IsUnique = true,Order = 0)]
        public long LedgerId { get; set; }
        public virtual Ledger fk_Ledger { get; set; }

        [ForeignKey("fk_Role"), Required, Index("IDX_LedgerRole_UniqueKey", IsUnique = true, Order = 1)]
        public long RoleId { get; set; }
        public virtual ConstantValue fk_Role { get; set; }

        public bool IsDefault { get; set; }
    }
}