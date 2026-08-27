using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mBrandMaster")]
    public class BrandMaster : AuditableEntity
    {
        [Column("BrandName"), Required, MaxLength(100), Index("IDX_mBrandMaster_BrandName", IsUnique = true)]
        public string BrandName { get; set; }

        [Column("BrandAbbr"), Required, MaxLength(100), Index("IDX_mBrandMaster_BrandAbbr", IsUnique = true)]
        public string BrandAbbr { get; set; }

        [Column("ManufacturerId")]
        public long ManufacturerId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ manufacturer.
        /// Of type 1001
        /// </summary>
        /// <value>The FK_ manufacturer.</value>
        /// 
        [ForeignKey("ManufacturerId")]
        public virtual GenericMaster fk_Manufacturer { get; set; }

        [Column("NatureId"), ForeignKey("fk_BrandNature")]
        public long? NatureId { get; set; }

        

        /// <summary>
        /// Gets or sets the FK_ brand nature.
        /// Of Type 1003
        /// </summary>
        /// <value>The FK_ brand nature.</value>

        public virtual GenericMaster fk_BrandNature { get; set; }
        /// <summary>
        /// Gets or sets the FK_ Pattern nature.
        /// Of Type xxx
        /// </summary>
        /// <value>The FK_ brand nature.</value>
        [Column("PatternId"), ForeignKey("fk_Pattern")]
        public long? PatternId { get; set; }
        public virtual GenericMaster fk_Pattern { get; set; }
        [Column("PlyRatingId"), ForeignKey("fk_PlyRating")]
        public long? PlyRatingId { get; set; }
        /// <summary>
        /// Gets or sets the ply ratting.
        /// Of Type 1002
        /// </summary>
        /// <value>The FK_ brand nature.</value>
        public virtual GenericMaster fk_PlyRating { get; set; }
        [Column("SizeId"), ForeignKey("fk_Size")]
        public long? SizeId { get; set; }
        /// <summary>
        /// Gets or sets the ply ratting.
        /// Of Type 1002
        /// </summary>
        /// <value>The FK_ brand nature.</value>
        public virtual GenericMaster fk_Size { get; set; }

        [Column("Remark"), MaxLength(255)]

        public string Remark { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

        public int BudgetedKmLife { get; set; } = 0;
        public decimal MinNSD { get; set; } = 0;
        [Column("STD")]
        public decimal StandardThreadDepth { get; set; } = 0;
        [Range(1135, 1136,ErrorMessage = "Invalid Related Type Defined. Type could be 1135 or 1136"),Required]
        public long RelatedTypeId { get; set; }
        [ForeignKey("RelatedTypeId")]
        public virtual ConstantValue fk_RelatedTypeId { get; set; }
    }
}