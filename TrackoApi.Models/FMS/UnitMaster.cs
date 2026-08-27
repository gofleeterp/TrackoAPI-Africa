using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mUnitMaster")]
    public class UnitMaster:AuditableEntity
    {
        [Index("IX_mUnitMaster_UnitKey",IsUnique = true,Order =0), MaxLength(200)]
        public string UnitName { get; set; }
        [Index("IX_mUnitMaster_UnitKey", IsUnique = true, Order = 1),MaxLength(200)]
        public string Alias { get; set; }

        [Index("IX_mUnitMaster_UnitKey", IsUnique = true, Order = 2)]
        public long? UnitCategoryId { get; set; }
        [ForeignKey("UnitCategoryId")]
        public virtual ConstantValue fk_UnitCategory { get; set; }


        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }
        public virtual List<UnitConverter> UnitConversions { get; set; }

        [MaxLength(200)]
        public string RefI { get; set; }
        [MaxLength(200)]
        public string RefII { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
    }
}