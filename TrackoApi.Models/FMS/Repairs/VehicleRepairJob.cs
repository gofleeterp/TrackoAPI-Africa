using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleRepairJob")]
    public class VehicleRepairJob:AuditableEntity,IValidatableObject
    {
        public long DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual DriverMaster fk_Driver { get; set; }
        public long? JobCardId { get; set; }
        [ForeignKey("JobCardId")]
        public virtual VehicleMovementLog fk_JobCard { get; set; }
        [MaxLength(500)]
        public string Complaint { get; set; }
        /// <summary>
        /// Gets or sets the mechanic identifier.
        /// Tyre Id 1096
        /// </summary>
        /// <value>The mechanic identifier.</value>
        public long? MechanicId { get; set; }
        [ForeignKey("MechanicId")]
        public virtual GenericMaster fk_Mechanic { get; set; }
        /// <summary>
        /// Gets or sets the job group identifier.
        /// TypeId 1078
        /// </summary>
        /// <value>The job group identifier.</value>
        public long? JobGroupId { get; set; }
        [ForeignKey("JobGroupId")]
        public virtual GenericMaster fk_JobGroup { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DriverId<=0)
            {
                yield return new ValidationResult("Selected Driver is Invalid",new []{ "DriverId" });
            }
        }
    }
}