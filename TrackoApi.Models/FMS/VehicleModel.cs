using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleModel")]
    public class VehicleModel : AuditableEntity
    {

        [Column("ModelName"), Required, MaxLength(100)]
        public string ModelName { get; set; }

        [Column("ModelAbbr"), Required, MaxLength(100)]
        public string  Abbr { get; set; }

        [Column("ManufacturerId"), ForeignKey("fk_Manufacturer"),Required]
        public long ManufacturerId { get; set; }
        public GenericMaster fk_Manufacturer { get; set; }

        public long NoOfTyres { get; set; } = 0;

        public long NoOfStphny { get; set; } = 0;
        public long NoOfBatteries { get; set; } = 0;
        public int NoOfFreeService { get; set; } = 0;
        public long? AxleTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the axle.
        /// Type Id 57
        /// </summary>
        /// <value>The type of the axle.</value>
        [ForeignKey("AxleTypeId")]
        public virtual ConstantValue fk_AxleType { get; set; }

        public long? WheelLayoutId { get; set; }
        [ForeignKey("WheelLayoutId")]
        public virtual ConstantValue fk_WheelLayout { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }

        [Column("ModelNatureId"), ForeignKey("fk_ModelNature")]
        public long? ModelNatureId { get; set; }
        public virtual ConstantValue fk_ModelNature { get; set; }

        public string ViewId { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
    }
}