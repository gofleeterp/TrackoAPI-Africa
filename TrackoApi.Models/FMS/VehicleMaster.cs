using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Base.Attributes;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.FMS.Driver;

namespace TrackoApi.Models.FMS
{
    [Table("mVehicleMaster")]
    public class VehicleMaster : AuditableEntity,IValidatableObject
    {
        [Key,Column("Id"),ForeignKey("fk_VehicleLedger")]
        public override long Id { get; set; }
        public Ledger fk_VehicleLedger { get; set; }
        [Column("VehicleNo"), Required, MaxLength(100),Index("IX_mVehicleMaster_VehicleNo_Unique", IsUnique = true)]
        public string VehicleNo { get; set; }

        [Column("VehicleRegNo"), Required, MaxLength(100), Index("IX_mVehicleMaster_VehicleRegNo_Unique", IsUnique = true)]
        public string VehicleRegNo { get; set; }

        [Column("YearOfManufacter"), Required]
        public int YearOfManufacter { get; set; }

        [Column("OfficeId"), Required, ForeignKey("Office")]
        public long? OfficeId { get; set; }

        public OfficeMaster Office { get; set; }

        [Column("OwnerPartyId"),ForeignKey("fk_VehicleOwner")]
        public long? OwnerPartyId { get; set; }
        public Ledger fk_VehicleOwner { get; set; }

        [Column("VehicleModelId"), Required, ForeignKey("fk_VehicleModel")]
        public long VehicleModelId { get; set; }
        public VehicleModel fk_VehicleModel { get; set; }

        [Column("RegistrationDate")]
        public DateTime? RegistrationDate { get; set; }

        [Column("PurchaseDate")]
        public DateTime? PurchaseDate { get; set; }

        [Column("PurchaseAmount")]
        public decimal PurchaseAmount { get; set; } = 0;

        [Column("SoldDate")]
        public DateTime? SoldDate { get; set; }

        [Column("SoldAmount")]
        public decimal SoldAmount { get; set; }= 0;

        [Column("ChassisNo")]
        [MaxLength(100)]
        public string ChassisNo { get; set; }
        [MaxLength(100)]
        [Column("EngineNo")]
        public string EngineNo { get; set; }

        [Column("GrossWeight")]
        public long? GrossWeight { get; set; }

        [Column("UnloadWeight")]
        public long? UnloadWeight { get; set; }

        [Column("VehicleTypeId"), ForeignKey("fk_VehicleType")]
        public long? VehicleTypeId { get; set; }
        public GenericMaster fk_VehicleType { get; set; }

        [Column("IsHireVehicle")]
        public bool IsHireVehicle { get; set; }

        public long? PassingAuthorityId { get; set; }
        [ForeignKey("PassingAuthorityId")]
        public virtual CityMaster fk_PassingAuthority { get; set; }
        
        //[Required]
        //public vwTyreChassisBill ChassisBill { get; set; }
        //public vwBatteryChassisBill ChassisBattery { get; set; }
        public virtual List<MasterAlias> Aliases { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        
        public virtual List<VehicleDueMapping> Dues { get; set; }
        public virtual List<VTSStatusLog> VTSLogs { get; set; }
        public vwFleetAccount AccountDetail { get; set; }
        public long? ChassisBatteryInfoId { get; set; }
        [ForeignKey("ChassisBatteryInfoId")]
        public virtual BatteryLogExtraInfo fk_ChassisBatteryInfo { get; set; }
        public long? ChassisTyreInfoId { get; set; }
        [ForeignKey("ChassisTyreInfoId")]
        public virtual TyreLogExtraInfo fk_ChassisTyreInfo { get; set; }

        public decimal VehicleLength { get; set; } = 0;
        public decimal VehicleHeight { get; set; } = 0;
        public decimal VehicleWidth { get; set; } = 0;
        public decimal FuelTankCapacity { get; set; } = 0;
        public bool IsGPSAttached { get; set; } = false;
        [MaxLength(100)]
        public string Ref1 { get; set; }
        [MaxLength(100)]
        public string Ref2 { get; set; }
        [MaxLength(100)]
        public string Ref3 { get; set; }
        [MaxLength(100)]
        public string Ref4 { get; set; }

        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_RefI { get; set; }
        public long? Ref2Id { get; set; }
        [ForeignKey("Ref2Id")]
        public virtual GenericMaster fk_RefII { get; set; }
        public long? Ref3Id { get; set; }
        [ForeignKey("Ref3Id")]
        public virtual GenericMaster fk_RefIII { get; set; }
        public decimal VehBudgDriverExp { get; set; } = 0;
        public decimal VehBudgTyreExp { get; set; } = 0;
        public decimal VehBudgRepairExp { get; set; } = 0;
        public decimal VehBudgPaperExp { get; set; } = 0;
        public decimal VehBudgTripExp { get; set; } = 0;
        public decimal VehBudgEMIExp { get; set; } = 0;
        public decimal VehBudgOtherExp { get; set; } = 0;
        public long? GPSVendorId { get; set; }
        public decimal LoadedMileage { get; set; } = 0;
        public decimal EmptyMileage { get; set; } = 0;
        public decimal Capacity { get; set; } = 0;
        public int ODOMeter { get; set; }
        [ForeignKey("CapacityUnitId")]
        public virtual UnitMaster fk_CapacityUnit { get; set; }
        public long? CapacityUnitId { get; set; }

        [ForeignKey("GPSVendorId")]
        public virtual Ledger fk_GPSVendor { get; set; }

        public long? FinancierId { get; set; }
        [ForeignKey("FinancierId")]
        public virtual Ledger fk_Financier { get; set; }

        public long? DealerId { get; set; }
        [ForeignKey("DealerId")]
        public virtual Ledger fk_Dealer { get; set; }

        [MaxLength(50)]
        public string DebitCardNo { get; set; }

        [MaxLength(50)]
        public string DebitCardAcNo { get; set; }

        [MaxLength(100)]
        public string FastTagNo { get; set; }

        [MaxLength(100)]
        public string FastTagAcNo { get; set; }

        public DateTimeOffset? LastFastTagSyncDate { get; set; }
        public decimal FastTagBalance { get; set; }

        [MaxLength(100)]
        public string FuelCardNo { get; set; }

        [MaxLength(100)]
        public string FuelCardAcNo { get; set; }

        public bool IsDeactive { get; set; }


        #region Trailor Capacity
        public decimal Compartment1 { get; set; } = 0;
        public decimal Compartment2 { get; set; } = 0;
        public decimal Compartment3 { get; set; } = 0;
        public decimal Compartment4 { get; set; } = 0;
        public decimal Compartment5 { get; set; } = 0;
        public decimal Compartment6 { get; set; } = 0;
        public decimal Compartment7 { get; set; } = 0;

        public decimal Ullaje1 { get; set; } = 0;
        public decimal Ullaje2 { get; set; } = 0;
        public decimal Ullaje3 { get; set; } = 0;
        public decimal Ullaje4 { get; set; } = 0;
        public decimal Ullaje5 { get; set; } = 0;
        public decimal Ullaje6 { get; set; } = 0;
        public decimal Ullaje7 { get; set; } = 0;

        public decimal PMS { get; set; } = 0;/*For Petrol*/
        public decimal AGO { get; set; } = 0;/*For Other*/
        #endregion




        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(VehicleNo))
            {
                yield return new ValidationResult("Vehicle Number is Required",new []{ "VehicleNo" });
            }
            if (string.IsNullOrWhiteSpace(VehicleRegNo))
            {
                yield return new ValidationResult("Vehicle Number is Required", new[] { "VehicleNo" });
            }
        }
        public long? ViewId { get; set; }
        [MaxLength(255)]
        public string BatchId { get; set; }

        public virtual List<VehicleCardMapping> Cards { get; set; }

        
        public long? ModelNatureId { get; set; }
        [ForeignKey("ModelNatureId")]
        public virtual ConstantValue fk_ModelNature { get; set; }
        [MaxLength(200)]
        public string TrailorNo { get; set; }

        public long? TrailorId { get; set; }
        [ForeignKey("TrailorId")]
        public virtual VehicleMaster fk_Trailor { get; set; }

        public virtual List<VehicleTrailorMapping> Trailors { get; set; }
        public virtual List<VehicleTrailorMapping> Vehicles { get; set; }
        [Column("FuelTypeId")]
        public long? FuelTypeId { get; set; }
        [ForeignKey("FuelTypeId")]
        public virtual GenericMaster fk_FuelType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DbGeography GeographicPoint { get; set; }
        public string GPSLocation { get; set; }
        public DateTime? GPSTime { get; set; }
        public long? GPSId { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;

        public VehicleMaster()
        {

        }
    }
    
}

