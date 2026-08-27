using Newtonsoft.Json;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("mTenantConstantType")]
        public class TenantConstantType : WorkItemAuditableEntity
        {
            public TenantConstantType()
            {
                TenantConstantValues = new List<TenantConstantValue>();
            }
            [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
            public override long Id { get; set; }
            [Column("ConstantTypeAbbr"), Index("IX_ConstantType_TypeAbbr", IsUnique = true),
             MaxLength(100, ErrorMessage = "Abbreviation upto 100 chars long and is required."), Required]
            public string ConstantTypeAbbr { get; set; }

            [Column("ConstantTypeName"), Index("IX_ConstantType_TypeName", IsUnique = true), MaxLength(200), Required]
            public string ConstantTypeName { get; set; }

            [Column("ConstantTypeDesc"), MaxLength(200)]
            public string ConstantTypeDesc { get; set; }
            public virtual ICollection<TenantConstantValue> TenantConstantValues { get; set; }
        }
    
}
