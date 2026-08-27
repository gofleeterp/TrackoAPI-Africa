using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tTyreCheck")]
    public class TyreCheck:AuditableEntity,IValidatableObject
    {
        [Column("VehicleId"),ForeignKey("fk_Vehicle"),Required]
        public long VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("TyreId"), ForeignKey("fk_Tyre")]//, Required,Index("IX_TyreCheck_UniqueKey",IsUnique = true,Order = 0)] //Commented befause on the same day if user receipt the tyre and again issue the tyre to vehicle it cause Duplicate Data Issue
        public long TyreId { get; set; }
        public virtual TyreMaster fk_Tyre { get; set; }

        [Column("CheckDate"), DataType(DataType.DateTime)]//, Required, Index("IX_TyreCheck_UniqueKey", IsUnique = true, Order = 1)]
        public DateTime CheckDate { get; set; }

        [Column("WPID")]
        public long? WheelPositionId { get; set; }
        [ForeignKey("WheelPositionId")]
        public virtual GenericMaster fk_WheelPosition { get; set; }
        [Column("TreadDepth")]
        public decimal TreadDepth { get; set; } = 0;
        [Column("TreadDepth2")]
        public decimal TreadDepth2 { get; set; } = 0;
        [Column("TreadDepth3")]
        public decimal TreadDepth3 { get; set; } = 0;
        [Column("TreadDepth4")]
        public decimal TreadDepth4 { get; set; } = 0;

        public long? JobCardId { get; set; }
        [ForeignKey("JobCardId")]
        public virtual VehicleMovementLog JobCard { get; set; }

        public bool IsStephney { get; set; } = false;
        [Column("AirPressure")]
        public decimal AirPressure { get; set; } = 0;

        [Column("KmRun")]
        public long? KmRun { get; set; }

        [Column("Remarks")]
        [MaxLength(200)]
        public string Remarks { get; set; }
        [Column("PreviousLogId")]
        public long? PreviousLogId { get; set; }
        [ForeignKey("PreviousLogId")]
        public virtual TyreCheck fk_PreviousLog { get; set; }
        [Column("NextLogId")]
        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public TyreCheck fk_NextLog { get; set; }
        public long? ViewId { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckDate > DateTime.Now.AddMinutes(30))
            {
                yield return new ValidationResult($"Fueture Date Transaction is not allowed", new[] { "CheckDate" });
            }
        }
    }
}
