using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS.Repairs
{
    //[Table("mFMSStorageBin")]
    //public class FMSStorageBin : AuditableEntity
    //{
    //    public long StoreId { get; set; }
    //    [ForeignKey("StoreId")]
    //    public virtual Ledger fk_Store { get; set; }

    //    public string BinName { get; set; }

    //    public long RoomId { get; set; }
    //    [ForeignKey("RoomId")]
    //    public virtual GenericMaster fk_Room { get; set; }

    //    /// <summary>
    //    /// Gets or sets the coordinate.
    //    /// Refer to https://help.sap.com/saphelp_erp60_sp/helpdata/en/c6/f839134afa11d182b90000e829fbfe/content.htm
    //    /// </summary>
    //    /// <example>The coordinate 01-02-03 for example, can refer to a storage bin in row 1, stack 2, and level 3.</example>
    //    /// <value>The coordinate.</value>
    //    public string Coordinate { get; set; }

    //    public decimal QtyCapacity { get; set; }
    //    public decimal WeightCapacity { get; set; }
    //}
    [Table("mSBM")]
    public class StoreBinMaster : AuditableEntity
    {
        public long StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Ledger fk_Store { get; set; }
        [MaxLength(100)]
        public string BinName { get; set; }

        public long RoomId { get; set; }
        [ForeignKey("RoomId")]
        public virtual GenericMaster fk_Room { get; set; }

        /// <summary>
        /// Gets or sets the coordinate.
        /// Refer to https://help.sap.com/saphelp_erp60_sp/helpdata/en/c6/f839134afa11d182b90000e829fbfe/content.htm
        /// </summary>
        /// <example>The coordinate 01-02-03 for example, can refer to a storage bin in row 1, stack 2, and level 3.</example>
        /// <value>The coordinate.</value>
        [MaxLength(200)]
        public string Coordinate { get; set; }

        public decimal QtyCapacity { get; set; }
        public decimal WeightCapacity { get; set; }
    }
}
