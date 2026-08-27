using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.AMS
{
    [Table("tVTGMapping")]
    public class VoucherTypeGroupMapping:AuditableEntity
    {
        [Required]
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ViewField fk_Type { get; set; }
        public long? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual AccountGroup fk_Group { get; set; }

        public long? LedgerRoleId { get; set; }
        [ForeignKey("LedgerRoleId")]
        public virtual ConstantValue fk_LedgerRole { get; set; }
        [MaxLength(300)]
        public string Include { get; set; }
        [MaxLength(300)]
        public string Exclude { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public long? VoucherTypeId { get; set; }
        [ForeignKey("VoucherTypeId")]
        public virtual VoucherType fk_VoucherType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

        public long? ViewId { get; set; }
        [ForeignKey("ViewId")]
        public virtual ApiView fk_View { get; set; }
    }
}
