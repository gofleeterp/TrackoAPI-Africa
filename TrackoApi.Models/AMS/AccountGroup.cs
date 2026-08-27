using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.AMS
{

    [Table("mAccountGroup")]
    public class AccountGroup:AuditableEntity
    {
        [Column("GroupName"), Index("IDX_mAccountGroup_GroupName", IsUnique = true), Required, MaxLength(200)]
        public string GroupName { get; set; }

        [Column("GroupAbbr"), Index("IDX_mAccountGroup_GroupAbbr", IsUnique = true), Required, MaxLength(200)]
        public string Alias { get; set; }
        [ForeignKey("fk_ParentGroup")]
        public long? ParentGroupId { get; set; }
        public virtual AccountGroup fk_ParentGroup { get; set; }
        public virtual List<AccountGroup> ChildAccountGroups { get; set; }
        public virtual List<Ledger> Ledgers { get; set; }
        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
        public bool IsRevenue { get; set; } = false;
    }

    public class AccountParentChild
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RowID { get; set; }
        public long GroupId { get; set; }

        public string GroupName { get; set; }
        public long? ParentGroupId { get; set; }
        public string ParentGroupName { get; set; }
        public long Level { get; set; }
        public long GrandParentId { get; set; }
    }
}
