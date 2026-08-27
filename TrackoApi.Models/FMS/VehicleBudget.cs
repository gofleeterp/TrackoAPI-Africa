using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleBudget")]
    public class VehicleBudget : AuditableEntity
    {
        [Column("FromDate"), Required]
        public DateTime FromDate { get; set; }

        [Column("ToDate"), Required]
        public DateTime ToDate { get; set; }

        [Column("ParameterId"),Required,ForeignKey("fk_Parameter")]
        public long CalculatingParameterId { get; set; }

        public virtual ConstantValue fk_Parameter { get; set; }

        [Column("FactorId"), Required, ForeignKey("fk_Factor")]
        public long CalculatingFactorId { get; set; }
        public virtual ConstantValue fk_Factor { get; set; }

        [Column("CalculatedValue"), Required]
        public long CalculatedValue { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}