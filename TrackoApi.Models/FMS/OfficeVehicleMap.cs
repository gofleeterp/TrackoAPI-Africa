using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mOfficeVehicleMapping")]
    public class OfficeVehicleMap : AuditableEntity
    {
        [Column("VehicleId"), ForeignKey("fk_Vehicle"), Required]
        public long VehicleId { get; set; }
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("OfficeId"), ForeignKey("fk_Office"), Required]
        public long OfficeId { get; set; }
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("MappingDate"), Required]
        public DateTime MappingDate { get; set; }

        public string BatchId { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}