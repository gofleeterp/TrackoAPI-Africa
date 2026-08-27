using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleOwnerMapping")]
    public class VehicleOwnerMapping : AuditableEntity
    {
        [Column("VehicleId"), ForeignKey("fk_Vehicle"),Required]
        public long VehicleId { get; set; }

        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("OwnerId"), ForeignKey("fk_Owner"),Required]

        public long OwnerId { get; set; }
        public virtual Ledger fk_Owner { get; set; }

        [Column("FromDate"), Required]

        public DateTime FromDate { get; set; }

        [Column("ToDate"), Required]

        public DateTime ToDate { get; set; }

        [Column("VehicleNo"),Required]
        [MaxLength(100)]
        public string NewVehicleNo { get; set; }
        [MaxLength(100)]
        [Column("VehicleRegNo"), Required]
        public string NewVehicleRegistrationNo { get; set; }
        [MaxLength(200)]
        [Column("Remarks")]
        public string Remarks { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }

    }
}