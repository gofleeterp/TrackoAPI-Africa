using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mPMSchedule")]
    public class PMSchedule : AuditableEntity
    {
        [Column("SchedulePMId"), Required, ForeignKey("fk_PM")]
        public long SchedulePMId { get; set; }
        public virtual PMMaster fk_PM { get; set; }

        [Column("ScheduleDate"), Required, DataType((DataType.Date))]
        public DateTime ScheduleDate { get; set; }


        [Column("ClassId"), Required,ForeignKey("fk_Class")]
        public long ClassId { get; set; }
        public virtual ObjectClass fk_Class { get; set; }

        [Column("ExpiryDate"),DataType((DataType.Date))]
        public DateTime? ExpiryDate { get; set; }

        public long ScheduleKm { get; set; }
        public long ScheduleDays { get; set; }

        public long AlertDays { get; set; }
        public long AlertKm { get; set; }

        [Column("ScheduleNextId"), ForeignKey("fk_ScheduleNext")]
        public long? ScheduleNextId { get; set; }
        public virtual PMSchedule fk_ScheduleNext { get; set; }

        [MaxLength(250)]
        public string Remark { get; set; }
        public long? ViewId { get; set; }
    }
}

