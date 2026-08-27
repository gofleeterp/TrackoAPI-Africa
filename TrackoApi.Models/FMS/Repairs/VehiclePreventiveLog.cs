using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tVehiclePMLog")]
    public class VehiclePreventiveLog:AuditableEntity,IValidatableObject
    {
        public long PMId { get; set; }
        [ForeignKey("PMId")]
        public virtual PMMaster fk_PMMaster { get; set; }
        
        public long ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual PMSchedule fk_PMSchedule { get; set; }



        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }
        [Column("ClassId"), Required, ForeignKey("fk_Class")]
        public long ClassId { get; set; }
        public virtual ObjectClass fk_Class { get; set; }
        public long? JobCardId { get; set; }
        [ForeignKey("JobCardId")]
        public virtual VehicleMovementLog fk_JobCard { get; set; }

        public long? BillId { get; set; }
        [ForeignKey("BillId")]
        public virtual SpareLogExtraInfo fk_BillId { get; set; }


        public long? NewPMId { get; set; }
        [ForeignKey("NewPMId")]
        public virtual PMMaster fk_NewPMMaster { get; set; }

        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public virtual VehiclePreventiveLog fk_NextLog { get; set; }
        public long? PreviousLogId { get; set; }
        [ForeignKey("PreviousLogId")]
        public virtual VehiclePreventiveLog fk_PreviousLog { get; set; }
        [Required]
        public DateTime JobDate { get; set; }
        public int StartKM { get; set; } = 0;
        [Required]
        public DateTime DueDate { get; set; }
        public DateTime? DueAlertDate { get; set; }
        public int DueKM { get; set; }
        public int DueDays { get; set; }
        public int AlertKM { get; set; }
        public int AlertDays { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DueDate <= JobDate)
            {
                yield return new ValidationResult($"Due Date should be greater than JobCard Date",new []{ "DueDate" });
            }
            if ((DueKM + DueDays) == 0)
            {
                yield return new ValidationResult($"DueKM or DueDays should be greater than zero(0).", new[] { "DueKM", "DueDays" });
            }
            if (NewPMId.GetValueOrDefault(0) == 0)
            {
                NewPMId = PMId;
            }
            if (!DueAlertDate.HasValue)
            {
                DueAlertDate = DueDate.AddDays(-AlertDays);
            }
        }
    }
}