using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS.Repairs;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mSpareMaster")]
    public class SpareMaster : AuditableEntity
    {
        [Column("SpareName"), MaxLength(100), Index("IDX_mSpareMaster_SpareName", IsUnique = true)]
        public string SpareName { get; set; }

        [Column("SpareAbbr"), MaxLength(100), Index("IDX_mSpareMaster_SpareAbbr", IsUnique = true)]
        public string SpareAbbr { get; set; }

        /// <summary>
        /// Gets or sets the spare group identifier.
        /// </summary>
        /// <value>The spare group identifier.</value>
        [Column("SpareGroupId")]
        public long? SpareGroupId { get; set; }
        ///// <summary>
        ///// Gets or sets the FK_ spare group.
        ///// Constant Id equal 1078
        ///// </summary>
        ///// <value>The FK_ spare group.</value>
        [ForeignKey("SpareGroupId")]
        public virtual GenericMaster fk_SpareGroup { get; set; }

        public long? BaseUnitId { get; set; }
        [ForeignKey("BaseUnitId")]
        public virtual UnitMaster fk_BaseUnit { get; set; }

        [Column("SpareNatureId")]
        public long? SpareNatureId { get; set; } //Spare/Labour
        [ForeignKey("SpareNatureId")]
        public virtual ConstantValue fk_SpareNature { get; set; }

        [Column("SpareTypeId"), ForeignKey("fk_SpareType")]
        public long? SpareTypeId { get; set; } //Used In Vehicle / Tyre
        public virtual ConstantValue fk_SpareType { get; set; }

        [Column("Remarks"), MaxLength(500)]
        public string Remarks { get; set; }


        [Column("AfterUsedId"), ForeignKey("fk_AfterUse")]
        public long? AfterUseId { get; set; } //Refurbish/Scrap/Loose Scrap/NA        
        /// <summary>
        /// Gets or sets the FK_ after use.
        /// Constant Type eq 60
        /// </summary>
        /// <value>The FK_ after use.</value>
        public virtual ConstantValue fk_AfterUse { get; set; }

        public bool IsKit { get; set; } = false;

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        public bool Monitoring { get; set; } = false;

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public virtual List<SpareUnitMapping> Units { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

        public virtual List<MasterAlias> Aliases { get; set; }
        public List<SpareBinMapping> Bins { get; set; }
        public long? ViewId { get; set; }

        public long? CNMaterialId { get; set; }
        [ForeignKey("CNMaterialId")]
        public virtual MaterialMaster fk_CNMaterial { get; set; }

        public string JsonData { get; set; } = "[]"; /*for ZRA*/

    }
}