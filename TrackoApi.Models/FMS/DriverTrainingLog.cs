using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tDriverTrainingLog")]
    public class DriverTrainingLog : AuditableEntity
    {
        [Column("DriverId"), Required]
        public long DriverId { get; set; }
        [ForeignKey("DriverId")]
        public DriverMaster fk_Driver { get; set; }

        [Column("TrainingTypeId"), Required]
        
        public long TrainingTypeId { get; set; }
        [ForeignKey("TrainingTypeId")]
        public GenericMaster fk_TrainingType { get; set; }


        [Column("GradeId")]
        public long? GradeId { get; set; }
        [ForeignKey("GradeId")]
        public GenericMaster fk_Grade { get; set; }


        [Column("StartDate"), Required]
        public DateTime StartDate { get; set; }

        [Column("EndDate"), Required]
        public DateTime? EndDate { get; set; }
        [MaxLength(500)]
        public string Remark { get; set; }

        [MaxLength(100)]
        public string RefNo { get; set; }
        [MaxLength(100)]
        public string CertificateNo { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }
    }
}
