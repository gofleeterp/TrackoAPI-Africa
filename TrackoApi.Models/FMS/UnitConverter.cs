using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS
{
    [Table("tUnitConverter")]
    public class UnitConverter : AuditableEntity
    {
        //[Column("SpareId"), Index("IDX_mSpareConverter_Unique", 1, IsUnique = true)]
        //public long SpareId { get; set; }
        //[ForeignKey("SpareId")]
        //public virtual SpareMaster fk_Spare { get; set; }
        [Column("FromUnitId"), Index("IDX_mUnitConverter_Unique", 1, IsUnique = true)]
        public long FromUnitId { get; set; }
        [ForeignKey("FromUnitId")]
        public virtual UnitMaster fk_FromUnit { get; set; }

        [Column("ToUnitId"), Index("IDX_mUnitConverter_Unique", 2, IsUnique = true)]
        public long ToUnitId { get; set; }
        [ForeignKey("ToUnitId")]
        public virtual UnitMaster fk_ToUnit { get; set; }

        [Column("MultiplyFactor")]
        public decimal MultiplyFactor { get; set; }
    }
    [Table("tSpareUnitMapping")]
    public class SpareUnitMapping:AuditableEntity
    {
        public long SpareId { get; set; }
        [ForeignKey("SpareId")]
        public virtual SpareMaster fk_Spare { get; set; }
        public long UnitId { get; set; }
        public virtual UnitMaster fk_Unit { get; set; }
    }
}