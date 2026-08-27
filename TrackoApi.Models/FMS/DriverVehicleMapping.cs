using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tDriverVehicleMapping")]
    public class DriverVehicleMapping: AuditableEntity
    {
        [Column("VehicleId")]
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("DriverId"), Required]
        public long DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual DriverMaster fk_Driver { get; set; }

        [Column("StatusDate"), Required]
        public DateTime StatusDate { get; set; }

        [Column("DriverRoleId")]
        public long? DriverRoleId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ driver role.
        /// ContantTypeId 62
        /// </summary>
        /// <value>The FK_ driver role.</value>
        [ForeignKey("DriverRoleId")]
        public virtual ConstantValue fk_DriverRole { get; set; }

        [Column("DriverStatusId"), Required]
        public long DriverStatusId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ driver status.
        /// ConstantTypeId 61
        /// </summary>
        /// <value>The FK_ driver status.</value>
        [ForeignKey("DriverStatusId")]
        public virtual ConstantValue fk_DriverStatus { get; set; }

        [Column("DriverReasonId")]
        public long? DriverReasonId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ driver reason.
        /// ConstantId 1231
        /// </summary>
        /// <value>The FK_ driver reason.</value>
        [ForeignKey("DriverReasonId")]
        public virtual GenericMaster fk_DriverReason { get; set; }

        [Column("TripLogId")]
        public long? TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }

        [Column("Remark"), MaxLength(500)]
        public string Remark { get; set; }

        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public virtual DriverVehicleMapping fk_NextLog { get; set; }
        public long? PreviousLogId { get; set; }
        [ForeignKey("PreviousLogId")]
        public virtual DriverVehicleMapping fk_PreviousLog { get; set; }
        public long? ViewId { get; set; }
        public long? VTSLogId { get; set; }
        [ForeignKey("VTSLogId")]
        public virtual VTSStatusLog fk_VTSLog { get; set; }
    }

    [Table("tDvrNextStatusMapping")]
    public class DriverNextStatusMapping : Entity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }

        [Column("CurrentStatusId"), Required,Index("IDX_tDvrNextStatusMapping_Unique",IsUnique = true,Order = 0)]
        public long CurrentStatusId { get; set; }
        [ForeignKey("CurrentStatusId")]
        public virtual ConstantValue fk_CurrentStatus { get; set; }

        [Column("NextStatusId"), Required, Index("IDX_tDvrNextStatusMapping_Unique", IsUnique = true, Order = 2)]
        public long NextStatusId { get; set; }
        [ForeignKey("NextStatusId")]
        public virtual ConstantValue fk_NextStatus { get; set; }
    }

}
