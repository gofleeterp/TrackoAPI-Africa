using TrackoApi.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleClass")]
    public class VehicleClass : AuditableEntity
    {

        [Column("ClassName"), Required]
        [MaxLength(200)]
        public string ClassName { get; set; }


        [Column("CategoryId"), Required]
        public long CategoryId { get; set; }
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
