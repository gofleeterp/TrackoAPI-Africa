using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS.Repairs
{
    [Table("mSIL")]
    public class SpareInventoryLevel : AuditableEntity
    {
        [Index("IX_mSIL_Unique",IsUnique = true,Order = 0)]
        public long StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Ledger fk_Store { get; set; }
        [Index("IX_mSIL_Unique", IsUnique = true, Order = 1)]
        public long SpareItemId { get; set; }
        [ForeignKey("SpareItemId")]
        public virtual SpareMaster fk_SpareItem { get; set; }
        [Column("MakeId"), Index("IX_mSIL_Unique", IsUnique = true, Order = 2)]
        public long? MakeId { get; set; }
        [ForeignKey("MakeId")]
        public virtual GenericMaster fk_Make { get; set; }

        public decimal MiniStock { get; set; } = 0;
        public decimal MaxStock { get; set; } = 0;
        public decimal ReorderLevel { get; set; } = 0;
        public decimal ReorderQty { get; set; } = 0;
        public int DeadStockDays { get; set; } = 0;
    }
}