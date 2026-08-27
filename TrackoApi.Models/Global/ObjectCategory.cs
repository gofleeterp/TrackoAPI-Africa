using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mObjectCategory")]
    public class ObjectCategory : AuditableEntity, IValidatableObject
    {

        [Column("CategoryName"), Index("IDX_mObjectCategory_CategoryName", IsUnique = true),Required,MaxLength(50)]
        public string CategoryName { get; set; }
        
        [Column("CategoryTypeId")]
        public long CategoryTypeId { get; set; }
        [ForeignKey("CategoryTypeId")]
        public virtual ConstantValue fk_CategoryType { get; set; }

        [Column("RoleTypeId")]
        public long RoleTypeId { get; set; }
        [ForeignKey("RoleTypeId")]
        public virtual ConstantValue fk_RoleType { get; set; }

        [Column("RoleId")]
        public long RoleId { get; set; }
        [MaxLength(200)]
        public string RoleName { get; set; }
        //[ForeignKey("RoleId")]
        //public virtual ConstantValue fk_Role { get; set; }

        //[Column("AccountGroupId")]
        //public long? AccountGroupId { get; set; }
        //[ForeignKey("fk_AccountGroup")]
        //public virtual AccountGroup fk_AccountGroup { get; set; }

        public bool IsVisibility { get; set; } = true;
        public bool IsReserved { get; set; } = false;

        public List<ObjectClass> ObjectClasses { get; set; }

        public List<ObjectClassMap> Objects { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((ObjectState == ObjectState.Deleted || ObjectState == ObjectState.Modified) && IsReserved)
            {
                yield return new ValidationResult("Cannot update or delete built-in Object.");
            }
            if (RoleId == 0)
            {
                yield return new ValidationResult("Role can't be null.");
            }
        }
    }
}
