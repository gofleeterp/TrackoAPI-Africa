using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleTrailorMapping")]
    public class VehicleTrailorMapping : AuditableEntity
    {
        [Column("VehicleId"), Required, Index("Duplicate_Trailor_Mapping", IsUnique = true,Order = 1)]
        public long VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("TrailorId"), Required, Index("Duplicate_Trailor_Mapping", IsUnique = true, Order = 2)]
        public long TrailorId { get; set; }
        [ForeignKey("TrailorId")]
        public virtual VehicleMaster fk_Trailor { get; set; }

        
        [Column("OnDate"), Required, Index("Duplicate_Trailor_Mapping", IsUnique = true, Order = 3), DataType(DataType.Date)]
        public DateTime OnDate { get; set; }
        
        public DateTime? OffDate { get; set; }

        [Column("Remark"),MaxLength(1000)]
        public string Remark { get; set; }
    }
}
