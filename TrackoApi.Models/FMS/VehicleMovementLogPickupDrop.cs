using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tPickDroplog")]
    public class VehicleMovementLogPickupDrop : AuditableEntity
    {
        [Column("TriplogId"),ForeignKey("fk_Triplog")]
        public long TriplogId { get; set; }

        [Column("TripNatureId"), ForeignKey("fk_TripNature")]
        public long? TripNatureId { get; set; } //Loaded//Empty//ORM       
        /// <summary>
        /// Gets or sets the FK_ trip nature.
        /// Constant TypeId 72
        /// </summary>
        /// <value>The FK_ trip nature.</value>
        public virtual ConstantValue fk_TripNature { get; set; }

        [Column("VehicleAvg")]
        public decimal VehicleAvg { get; set; } = 0;

        [Column("FuelQty")]
        public decimal FuelQty { get; set; } = 0;


        public virtual VehicleMovementLog fk_Triplog { get; set; }
        [Column("OriginLocationId")]
        public long OriginLocationId { get; set; }
        [ForeignKey("OriginLocationId")]
        public virtual CityMaster fk_OriginLocation { get; set; }
        [Column("CityId")]
        public long CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual CityMaster fk_City { get; set; }
        [Column("TypeId"), ForeignKey("fk_Type")]
        public long TypeId { get; set; }//Pickup,Drop,Passthrough & PickupDrop, ConstantType:50
        public virtual ConstantValue fk_Type { get; set; }
        public int Order { get; set; }
        public int KM { get; set; }
        public decimal TravalTime { get; set; }
        public decimal StopageTime { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public DbGeography GeographyPoint { get; set; }
        [MaxLength(200)]
        public string HangfireJobId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Latitude > 0 || Longitude > 0 && GeographyPoint == null)
            {
                string errorMessage = "";
                try
                {
                    GeographyPoint = DbGeography.FromText($"POINT({Latitude} {Longitude})", 24378);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.GetBaseException().Message;
                }
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    yield return new ValidationResult(errorMessage, new[] { "Latitude", "Longitude" });
                }

            }
        }

    }

}