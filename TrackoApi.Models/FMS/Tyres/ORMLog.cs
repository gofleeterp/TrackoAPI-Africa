using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tORMLog")]
    public class ORMLog : AuditableEntity
    {
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        [MaxLength(100)]
        public string ORMNo { get; set; }
        public DateTime ORMDate { get; set; }
        
        public long VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }
        public long? SpareGroupId { get; set; } //Spare Group
        [ForeignKey("SpareGroupId")]
        public virtual GenericMaster fk_SpareGroup { get; set; }

        public long ORMOriginId { get; set; } 
        [ForeignKey("ORMOriginId")]
        public virtual ConstantValue fk_ORMOrigin { get; set; }

        public long? NatureId { get; set; } //Vehicle/ Tyre
        [ForeignKey("NatureId")]
        public virtual GenericMaster fk_Nature { get; set; }
        public long ORMTypeId { get; set; } //Accidental/Breakdown
        [ForeignKey("ORMTypeId")]
        public virtual ConstantValue fk_ORMType { get; set; }

        public long PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public virtual CityMaster fk_Place { get; set; }
        public long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual DriverMaster fk_Driver { get; set; }
        public DateTime? OnRoadDate { get; set; }
        public long? SupervisorId { get; set; }
        [ForeignKey("SupervisorId")]
        public virtual GenericMaster fk_Supervisor { get; set; }
        public decimal Amount { get; set; } = 0;
        public long? TriplogId { get; set; }
        [ForeignKey("TriplogId")]
        public virtual VehicleMovementLog fk_Triplog { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
        [MaxLength(200)]
        public string Comments { get; set; }
        public DateTime? VerificationDate { get; set; }
        [MaxLength(100)]
        public string VerifiedBy { get; set; }
        [MaxLength(200)]
        public string VerificationRemarks { get; set; }

       
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }
    }
}