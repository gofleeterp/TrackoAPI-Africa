using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mSpareFPosition")]
    public class SpareFitmentPosition : AuditableEntity
    {
        [Column("PostionId")]//, Index("IDX_mSpareFPostion_Unique", 1, IsUnique = true),
        [ ForeignKey("fk_FitmentPostion"),Required]
        public long FitmentPositionId { get; set; }

        public virtual GenericMaster fk_FitmentPostion { get; set; }

        [Column("SpareId")]//, Index("IDX_mSpareFPostion_Unique", 2, IsUnique = true),
        [ForeignKey("fk_Spare"), Required]
        public long SpareId { get; set; }

        public virtual GenericMaster fk_Spare { get; set; }
    }
}