using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("mTenantConstantValue")]
        public class TenantConstantValue : WorkItemAuditableEntity
        {
            [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
            public override long Id { get; set; }
            [Column("ConstantAbbr"), MaxLength(100)]
            public string ConstantAbbr { get; set; }

            [Column("ConstantName"), Index("IX_ConstantValue_Name", IsUnique = true, Order = 1), MaxLength(100), Required]
            public string ConstantName { get; set; }

            [Column("ConstantTypeId"), ForeignKey("fk_ConstantType"), Required, Index("IX_ConstantValue_Name", IsUnique = true, Order = 2)]
            public long ConstantTypeId { get; set; }

            public virtual TenantConstantType fk_ConstantType { get; set; }

            [Column("ConstantRemarks"), MaxLength(200)]
            public string ConstantRemarks { get; set; }

            [Column("Visiblity")]
            public bool Visiblity { get; set; }
            public string Ref1 { get; set; }
            public string Ref2 { get; set; }
            public string Ref3 { get; set; }
            public string Ref4 { get; set; }

        }
    
}
