using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.BMS
{
    [Table("mHireVehicle")]
    public class HireVehicle : AuditableEntity
    {
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_office { get; set; }
        [Index("IX_HireVehicle_Unique",IsUnique = true),MaxLength(200)]
        public string VehicleNo { get; set; }

        [Column("VehicleTypeId")]
        public long? VehicleTypeId { get; set; }
        [ForeignKey("VehicleTypeId")]
        public GenericMaster fk_VehicleType { get; set; }
        [MaxLength(100)]
        public string RegistrationNo { get; set; }

        [Column("VehicleModelId")]
        public long? VehicleModelId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ vehicle model.
        /// </summary>
        /// <value>The FK_ vehicle model.</value>
        [ForeignKey("fk_VehicleModel")]
        public VehicleModel fk_VehicleModel { get; set; }
        [MaxLength(100)]
        public string Owner { get; set; }
        [MaxLength(300)]
        public string OwnerAddress { get; set; }

        [Column("HirePartyId")]
        public long? HirePartyId { get; set; }
        [ForeignKey("HirePartyId")]
        public Ledger fk_HireParty { get; set; }

        public long? GPSVendorId { get; set; }
        [ForeignKey("GPSVendorId")]
        public virtual Ledger fk_GPSVendor { get; set; }
        [MaxLength(100)]
        public string EngineNo { get; set; }
        [MaxLength(100)]
        public string ChassisNo { get; set; }
        [MaxLength(100)]
        public string GpsAlias { get; set; }
        public bool IsBlackListed { get; set; }
        public long? ViewId { get; set; }

        [MaxLength(100)]
        public string Ref1 { get; set; }
        [MaxLength(100)]
        public string Ref2 { get; set; }
        /// <summary>
        /// ConstantValue Id= 1470, and ConstantType Id=44 
        /// </summary>

        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_RefI { get; set; }
        public long? Ref2Id { get; set; }

        /// <summary>
        /// ConstantValue Id= 1471, and ConstantType Id=44 
        /// </summary>
        [ForeignKey("Ref2Id")]
        public virtual GenericMaster fk_RefII { get; set; }
        public double ODOMeter { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DbGeography GeographicPoint { get; set; }
        public string GPSLocation { get; set; }
        public DateTime? GPSTime { get; set; }
        public long? GPSId { get; set; }
    }
}