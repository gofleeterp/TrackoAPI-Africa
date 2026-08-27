using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleFuelBudget")]
    public class VehicleFuelBudget : AuditableEntity
    {
        [Column("FromDate"), Required]
        public DateTime FromDate { get; set; }

        [Column("ToDate"), Required]
        public DateTime ToDate { get; set; }
        [Column("ObjectClassId"), Required]
        public long ObjectClassId { get; set; }
         [ForeignKey("ObjectClassId")]
        public virtual ObjectClass fk_ObjectClass { get; set; }
        //[Column("ClassId"), Required,ForeignKey("fk_Class")]
        //public long ClassId { get; set; }

        //public virtual VehicleClass fk_Class {get;set;}

        [Column("NormalAvg")]
        public decimal NormalAvg { get; set; } = 0;

        [Column("EmptyAvg")]
        public decimal EmptyAvg { get; set; } = 0;

        [Column("ReferAvg")]
        public decimal ReferAvg { get; set; } = 0;

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }

    }
}