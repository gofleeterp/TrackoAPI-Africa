using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleCardMapping")]
    public class VehicleCardMapping: AuditableEntity
    {
        [Column("OfficeId")]
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("VehicleId"), Required,Index("Duplicate_Card_Mapping", IsUnique = true,Order = 1)]
        public long VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("DriverId")]
        public long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual DriverMaster fk_Driver { get; set; }


        [Column("CardId"), Required]
        [Index("Duplicate_Card_Issue", IsUnique = true, Order = 1)]
        public long CardId { get; set; }
        [ForeignKey("CardId")]
        public virtual CardMaster fk_Card { get; set; }

        [Column("OnDate"), Required]
        public DateTime OnDate { get; set; }
        [Column("OnRemark"), MaxLength(500)]
        public string OnRemark { get; set; }
        [Index("Duplicate_Card_Issue", IsUnique = true, Order = 2)]
        [Index("Duplicate_Card_Mapping", IsUnique = true, Order = 2),DataType(DataType.Date)]
        public DateTime? OffDate { get; set; }
        [Column("OffRemark"), MaxLength(500)]
        public string OffRemark { get; set; }
        public long? ViewId { get; set; }
        public bool IsHotlisted { get; set; }
        [Index("Duplicate_Card_Mapping", IsUnique = true, Order = 3)]
        [Index("Duplicate_Card_Issue", IsUnique = true, Order = 3)]
        public long CardTypeId { get; set; }
        [ForeignKey("CardTypeId")]
        public virtual ConstantValue fk_CardType { get; set; }
    }
}
