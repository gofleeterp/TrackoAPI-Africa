using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleTailorMapping")]
    public class VehicleTailorMapping : AuditableEntity
    {
        [Column("VehicleId"), ForeignKey("fk_Vehicle"), Required]
        public long VehicleId { get; set; }

        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("TrailorId"), ForeignKey("fk_Trailor"), Required]

        public long TrailorId { get; set; }

        public virtual VehicleMaster fk_Trailor { get; set; }


        [Column("FromDate"), Required]

        public DateTime FromDate { get; set; }

        [Column("ToDate"), Required]

        public DateTime ToDate { get; set; }

        [Column("Remarks")]
        [MaxLength(300)]
        public string Remarks { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}