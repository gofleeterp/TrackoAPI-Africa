using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mPMMaster")]
    public class PMMaster : AuditableEntity
    {
        [Column("Name"), Required, Index("IDX_mPMMaster_Name", IsUnique = true), MaxLength(100)]
        public string Name { get; set; }

        [Column("Abbr"), Required, Index("IDX_mPMMaster_Abbr", IsUnique = true), MaxLength(100)]
        public string Abbr { get; set; }

        [Column("NatureID"), Required]
        public long NatureId { get; set; }
        [ForeignKey("fk_Nature")]
        public virtual ConstantValue fk_Nature { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
        [MaxLength(250)]
        public string Remark { get; set; }
        public long? ViewId { get; set; }
    }
}

