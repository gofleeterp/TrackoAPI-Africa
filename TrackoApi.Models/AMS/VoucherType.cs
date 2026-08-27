using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.AMS
{
    [Table("mVoucherType")]
    public class VoucherType:Base.Entity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }

        [Column("VoucherTypeName"),MinLength(3),MaxLength(50),Index("XI_VCHNAME",IsUnique = true)]
        public string VoucherTypeName { get; set; }
        [Column("Abbr"), MinLength(3), MaxLength(20), Index("XI_VCHABR", IsUnique = true)]
        public string Abbreviation { get; set; }
        [MaxLength(500)]
        public string NarrationTemplate { get; set; }

        public virtual List<VoucherTypeGroupMapping> GroupMappings { get; set; }
        public virtual List<ViewField> DefaultMappings { get; set; }
        public int VDRRequired { get; set; }//Count
        public int VDRequired { get; set; } = 2; //Count
        public bool IsAccountSubscribed { get; set; }
        [MaxLength(150)]
        public string RuleKey { get; set; }
        [IgnoreDataMember]
        public string SQLRule { get; set; }
        public bool IsApprovalRequired { get; set; }
        public long? NatureId { get; set; }
        [ForeignKey("NatureId")]
        public virtual ConstantValue fk_Nature { get; set; }
        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
    }
    
}
