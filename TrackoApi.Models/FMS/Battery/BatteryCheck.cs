using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS
{
    [Table("tBatteryCheck")]
    public class BatteryCheck:AuditableEntity,IValidatableObject
    {
        [Column("VehicleId"),ForeignKey("fk_Vehicle"),Required]
        public long VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("BatteryId"), ForeignKey("fk_Battery"), Required]
        public long BatteryId { get; set; }
        public virtual BatteryMaster fk_Battery { get; set; }

        [Column("CheckDate"),Required]
        public DateTime CheckDate { get; set; }
        /// <summary>
        /// Best Gravity Level: 1190-1230
        /// Recharable Level: less than 1190
        /// </summary>
        [Column("GravityLevel")]
        public int? GravityLevel { get; set; }

        [Column("IsWLC")]
        public bool IsWaterLevelChecked { get; set; }

        [Column("IsTCC")]
        public bool IsTerminalCarbonChecked { get; set; }

        public long? JobCardId { get; set; }
        [ForeignKey("JobCardId")]
        public virtual VehicleMovementLog JobCard { get; set; }

        

        [Column("Remarks")]
        [MaxLength(200)]
        public string Remarks { get; set; }
        public long? ViewId { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed ", new[] { "CheckDate" });
            }
        }
    }
}
