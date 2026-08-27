using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tDriverIncidentLog")]
    public class DriverIncidentLog : AuditableEntity,IValidatableObject
    {
        [Column("OfficeId"),Required]
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public OfficeMaster fk_Office { get; set; }

        [Column("DriverId"), Required]
        public long DriverId { get; set; }
        [ForeignKey("DriverId")]
        public DriverMaster fk_Driver { get; set; }

        [Column("RefNo"), StationaryCheck, Required, MaxLength(100)]
        public string RefNo { get; set; }

        [Column("RefDate"),Required]
        public DateTime RefDate { get; set; }

        [Column("VehicleId"),Required]
        public long VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public VehicleMaster fk_Vehicle { get; set; }

        [Column("TypeId"), Required]
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public GenericMaster fk_Type { get; set; }

        [Column("Remarks"),MaxLength(500) ]
        public string Remarks { get; set; }

        [Column("Comments"), MaxLength(500)]
        public string Comments { get; set; }

        public long DriverPoints { get; set; } = 0;

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RefDate.Date > DateTime.Today)
            {
                yield return new ValidationResult("Incident can't be logged in future date.", new []{ "RefDate" });
            }
            if (string.IsNullOrWhiteSpace(RefNo))
            {
                yield return new ValidationResult("Reference Number is required.", new[] { "RefNo" });
            }
        }
    }
}
