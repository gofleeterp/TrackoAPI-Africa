using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mTyreRubberType")]
    public class TyreRubberType : AuditableEntity
    {

        [Column("RubberType"), Required, MaxLength(100), Index("IDX_mTyreRubberType_RuberType", IsUnique = true)]
        public string RubberType { get; set; }

        [Column("ManufacturerName"), Required, MaxLength(100), Index("IDX_mTyreRubberType_Manufacturer", IsUnique = true)
        ]
        public string ManufacturerName { get; set; }

        [Column("BrandNatureId"), ForeignKey("fk_BrandNature")]
        public long BrandNatureId { get; set; }

        public GenericMaster fk_BrandNature { get; set; }

        [Column("StandardKm")]
        public long? StandardKm { get; set; }

        [Column("Remarks"), MaxLength(255)]
        public string Remarks { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
    }
}