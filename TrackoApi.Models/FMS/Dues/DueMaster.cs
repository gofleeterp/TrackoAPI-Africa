using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mDueMaster")]
    public class DueMaster : AuditableEntity
    {
        [Column("Name"), Required, Index("IDX_mDueMaster_Name", IsUnique = true), MaxLength(30)]
        public string Name { get; set; }

        [Column("Abbr"), Required, Index("IDX_mDueMaster_Abbr", IsUnique = true), MaxLength(30)]
        public string Abbr { get; set; }

        [Column("DueTypeId"), ForeignKey("fk_DueType")]
        public long DueTypeId { get; set; }
        public virtual ConstantValue fk_DueType { get; set; }
        [ForeignKey("DueAccountId")]
        public virtual Ledger fk_DueAccount { get; set; }
        public long? DueAccountId { get; set; }
        [ForeignKey("PayableAccountId")]
        public virtual Ledger fk_PayableAccount { get; set; }
        public long? PayableAccountId { get; set; }

        [ForeignKey("OthPayableAccountId")]
        public virtual Ledger fk_OthPayableAccount { get; set; }
        public long? OthPayableAccountId { get; set; }

        [ForeignKey("OtherAccountId")]
        public virtual Ledger fk_OtherAccount { get; set; }
        public long? PrepaidAccountId { get; set; }
        [ForeignKey("PrepaidAccountId")]
        public virtual Ledger fk_PrepaidAccount { get; set; }
        public long? OtherAccountId { get; set; }

        [ForeignKey("IGSTAccountId")]
        public Ledger fk_IGSTAccount { get; set; }
        public long? IGSTAccountId { get; set; }

        [ForeignKey("CGSTAccountId")]
        public Ledger fk_CGSTAccount { get; set; }
        public long? CGSTAccountId { get; set; }

        [ForeignKey("SGSTAccountId")]
        public Ledger fk_SGSTAccount { get; set; }
        public long? SGSTAccountId { get; set; }

        /// <summary>
        /// Gets or sets the renewal period in days.
        /// </summary>
        /// <value>The renewal period.</value>
        public int RenewalPeriod { get; set; } = 0;
        [Required]
        public long TimeUnitId { get; set; }
        [ForeignKey("TimeUnitId")]
        public virtual ConstantValue fk_TimeUnit { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }

        public bool IsReserved { get; set; } = false;
        public long? ViewId { get; set; }
    }
}
