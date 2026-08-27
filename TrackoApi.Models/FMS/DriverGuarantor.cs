using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS
{
    [Table("mDriverGuarantor")]
    public class DriverGuarantor : AuditableEntity,IValidatableObject
    {
        [Column("DriverId"), Required, ForeignKey("fk_Driver")]
        public long DriverId { get; set; }
        public DriverMaster fk_Driver { get; set; }

        [Column("GuarantorId"), Required, ForeignKey("fk_Guarantor")]
        public long GuarantorId { get; set; }
        public DriverMaster fk_Guarantor { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DriverId == GuarantorId)
            {
                yield return new ValidationResult("Driver cannot be guarantor of it's own.");
            }
        }
    }
}
