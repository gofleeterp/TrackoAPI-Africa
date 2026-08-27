using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;

namespace TrackoApi.Models.FMS.GPS
{
    [Table("tGPSKmLog")]
    public class GPSKmLog : AuditableEntity
    {
        [Column("VehicleId")]
        public long? VehicleId { get; set; }
        [ForeignKey("fk_Vehicle")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("HireVehicleId")]
        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }


        [Column("LogDate"), Required, Index("IX_GPSKmLog_UniqueKey", IsUnique = true, Order = 0), DataType(DataType.Date)]
        public DateTime LogDate { get; set; }

        [Column("VehicleNo"), Index("IX_GPSKmLog_UniqueKey", IsUnique = true, Order = 1)]
        [MaxLength(100)]
        public string VehicleNo { get; set; }

        [Column("KMPerHour")]
        public decimal KMPerHour { get; set; } = 0;

        [Column("GPSKm")]
        public decimal GPSKm { get; set; } = 0;
    }
}

