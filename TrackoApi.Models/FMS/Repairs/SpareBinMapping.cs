using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS.Repairs
{
    [Table("mSBinMap")]
    public class SpareBinMapping:AuditableEntity
    {
        [Column("StoreId"), Index("IX_tSBinMap_Unique", IsUnique = true, Order = 0)]
        public long StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Ledger fk_Store { get; set; }
        [Column("SpareItemId"), Index("IX_tSBinMap_Unique", IsUnique = true, Order = 1)]
        public long SpareItemId { get; set; }
        [ForeignKey("SpareItemId")]
        public virtual SpareMaster fk_SpareItem { get; set; }
        [Column("BinId"), Index("IX_tSBinMap_Unique", IsUnique = true, Order = 2)]
        public long? BinId { get; set; }
        [ForeignKey("BinId")]
        public virtual StoreBinMaster fk_Bin { get; set; }
    }
}