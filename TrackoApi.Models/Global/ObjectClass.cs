using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ModelValidations.Attributes;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mObjectClass")]
    public class ObjectClass : AuditableEntity, IValidatableObject
    {

        [Column("ClassName"), Index("IDX_mObjectClass_ClassName", IsUnique = true,Order = 0),Required,MaxLength(50)]
        public string ClassName { get; set; }

        [Column("CategoryId"), ForeignKey("Category"), Index("IDX_mObjectClass_ClassName", IsUnique = true, Order = 2)]
        public long CategoryId { get; set; }
        public virtual ObjectCategory Category { get; set; }

        [Column("RoleId"),Minimum(1)]
        public long RoleId { get; set; }
        public bool IsReserved { get; set; } = false;
        public List<ObjectClassMap> ObjectMappings { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((ObjectState == ObjectState.Deleted || ObjectState == ObjectState.Modified) && IsReserved)
            {
                yield return new ValidationResult("Cannot update or delete built-in Object Classes.");
            }
            if (RoleId <= 0)
            {
                yield return new ValidationResult("Invalid Role Selected.",new []{ "RoleId" });
            }
        }
    }
}
