using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ModelValidations.Attributes;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("tObjectClassMap")]
    public class ObjectClassMap : AuditableEntity
    {

        [Column("ClassId"), ForeignKey("fk_Class"), Required]
        public long ClassId { get; set; }
        public virtual ObjectClass fk_Class { get; set; }

        [Column("CategoryId"), ForeignKey("fk_Category"),Index("IDX_tObjectClassMap_UniqueKey",IsUnique = true,Order = 0)]
        public long CategoryId { get; set; }
        public virtual ObjectCategory fk_Category { get; set; }

        [Column("ObjectId"),  Required, Index("IDX_tObjectClassMap_UniqueKey", IsUnique = true, Order = 1),Minimum(1)]
        public long ObjectId { get; set; }
        [MaxLength(400)]
        public string ObjectName { get; set; }
        
    }
}
