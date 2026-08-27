using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mConstantType")]
    public class ConstantType : Entity
    {
        public ConstantType()
        {
            ConstantValues = new List<ConstantValue>();
        }
        [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [Column("ConstantTypeAbbr"), Index("IX_ConstantType_TypeAbbr", IsUnique = true),
         MaxLength(100, ErrorMessage = "Abbreviation upto 100 chars long and is required."), Required]
        public string ConstantTypeAbbr { get; set; }

        [Column("ConstantTypeName"), Index("IX_ConstantType_TypeName", IsUnique = true), MaxLength(100), Required]
        public string ConstantTypeName { get; set; }

        [Column("ConstantTypeDesc"), MaxLength(100)]
        public string ConstantTypeDesc { get; set; }
        public virtual ICollection<ConstantValue> ConstantValues { get; set; }
        public bool IsDepricated { get; set; }
    }
}
