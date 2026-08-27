using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleAccidentEst")]
    public class VehicleAccidentEstimate : AuditableEntity
    {
        [Column("AccidentClaimId"), ForeignKey("fk_VehicleAccidentClaim"), Index("IDX_mVTSStatusConfiguration_Name", IsUnique = true,Order = 1)]
        public long? AccidentClaimId { get; set; }
        public virtual VehicleAccidentClaim fk_VehicleAccidentClaim { get; set; }

        [Column("ItemLabourName"), Required, Index("IDX_mVTSStatusConfiguration_Name", IsUnique = true, Order = 2), MaxLength(50)]
        public string ItemLabourName { get; set; }
        [MaxLength(200)]
        public string  VendorName { get; set; }
        public decimal ClaimEstimate { get; set; }
        public decimal ClaimPassed { get; set; }
        public DateTime ExpCompletionDate { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }
    }
}