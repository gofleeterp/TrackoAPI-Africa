using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mDriverRelative")]
    public class DriverRelative : AuditableEntity
    {
        [Column("DriverId"), Required, ForeignKey("fk_Driver")]
        public long DriverId { get; set; }
        public virtual DriverMaster fk_Driver { get; set; }

        [Column("RelativeName"), Required, MaxLength(100)]
        public string RelativeName { get; set; }

        [Column("RelationTypeId"), Required, ForeignKey("fk_RelationType")]
        public long RelationTypeId { get; set; }
        public virtual ConstantValue fk_RelationType { get; set; }

        [Column("GenderId"), ForeignKey("fk_Gender"), Required]
        public long GenderId { get; set; }
        public virtual ConstantValue fk_Gender { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }
        public long? RelativeAge { get; set; }
        [MaxLength(25)]
        public string ContactNumber { get; set; }
        [MaxLength(500)]
        public string Address { get; set; }

        public bool IsNominee { get; set; }
    }
}
