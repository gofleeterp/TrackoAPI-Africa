using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleDueMapping")]
    public class VehicleDueMapping : AuditableEntity
    {
        [Column("VehicleId"), ForeignKey("fk_Vehicle"), Index("IDX_mVehicleDueMapping_VehicleId",1, IsUnique = true)]
        public long VehicleId { get; set; }

        public virtual VehicleMaster fk_Vehicle { get; set; }

        [Column("DueId"), ForeignKey("fk_Due"), Index("IDX_mVehicleDueMapping_VehicleId", 2, IsUnique = true)]
        
        public long DueId { get; set; }
        public virtual DueMaster fk_Due { get; set; }
        [Column("LastDueTransId")]
        public long? LastDueTransactionId { get; set; }
        [ForeignKey("LastDueTransactionId")]
        public virtual DueTransactionLog fk_LastDueTransaction{ get; set; }

        [Column("IsTrack")]
        public bool IsTrack { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }
    }
}