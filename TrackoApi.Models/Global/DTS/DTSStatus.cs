using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Models.Base;

using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global.DTS
{
    [Table("mDTSStatus")]
    public class DTSStatus : AuditableEntity
    {
        [Column("Name"), Required, MaxLength(100)]
        [Index("IDX_mDTSStatusConfiguration_Name", IsUnique = true, Order = 1)]
        public string Name { get; set; }

        [Column("Abbr"), Required, MaxLength(100)]
        [Index("IDX_mDTSStatusConfiguration_Abbr", IsUnique = true, Order = 1)]
        public string Abbr { get; set; }
        [Column("Alias"), Required, MaxLength(100)]
        [Index("IDX_mDTSStatusConfiguration_Alias", IsUnique = true, Order = 1)]
        public string Alias { get; set; }
        //[Column("TypeId"), Required]
        //public long TypeId { get; set; }
        //[ForeignKey("TypeId")]
        //public virtual ConstantValue fk_Type { get; set; }


        /// <summary>
        /// Gets or sets the fixed category identifier.
        /// constanttpyeid=91
        /// </summary>
        /// <value>The fixed category identifier.</value>
        public long? DateId { get; set; }
        [ForeignKey("DateId")]
        public virtual ConstantValue fk_Date { get; set; }
        /// <summary>
        /// Gets or sets the fixed category identifier.
        /// constanttpyeid=92
        /// </summary>
        /// <value>The fixed category identifier.</value>
        public long? FixedCategoryId { get; set; }
        [ForeignKey("FixedCategoryId")]
        public virtual ConstantValue fk_FixedCategory { get; set; }
        /// <summary>
        /// Gets or sets the nature identifier.
        /// constanttpyeid=93
        /// </summary>
        /// <value>The nature identifier.</value>

        public long? NatureId { get; set; }
        [ForeignKey("NatureId")]
        public virtual ConstantValue fk_Nature { get; set; }

        [Column("ReportCategoryId")]
        public long? ReportCategoryId { get; set; }
        [ForeignKey("ReportCategoryId")]
        public virtual GenericMaster fk_ReportCategory { get; set; }
        public long? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual DTSStatus fk_Parent { get; set; }

        public long? MonitorId { get; set; }
        [ForeignKey("MonitorId")]
        public virtual GenericMaster fk_Monitor { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }


        public int RGBColorCode { get; set; }
        public bool IsReserved { get; set; }
        public long? NextStatusId { get; set; }
        [ForeignKey("NextStatusId")]
        public virtual DTSStatus fk_NextStatus { get; set; }

        public virtual List<DTSStatusMapping> StatusMappings { get; set; }
        /// <summary>
        /// Gets or sets the ts type identifier.
        /// Tracking System Type Id e.g Vehicle or Consignment etc.
        /// </summary>
        /// <value>The ts type identifier.</value>
        [Index("IDX_mDTSStatusConfiguration_Name", IsUnique = true, Order = 2)]
        [Index("IDX_mDTSStatusConfiguration_Abbr", IsUnique = true, Order = 2)]
        [Index("IDX_mDTSStatusConfiguration_Alias", IsUnique = true, Order = 2)]
        public long TSTypeId { get; set; }
        [ForeignKey("TSTypeId")]
        public virtual ConstantValue fk_TStype { get; set; }
        [Index("IDX_mDTSStatusConfiguration_Name", IsUnique = true, Order = 3)]
        [Index("IDX_mDTSStatusConfiguration_Abbr", IsUnique = true, Order = 3)]
        [Index("IDX_mDTSStatusConfiguration_Alias", IsUnique = true, Order = 3)]
        public int TemplateId { get; set; }

        public bool IsInternalUse { get; set; } = false;
        public bool PreviousAsNext { get; set; } = false;
    }
}