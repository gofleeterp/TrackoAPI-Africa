using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.FMS
{
    [Table("mRouteWayPoint")]
    public class RouteWayPoint : AuditableEntity, IValidatableObject
    {
        [Index("IX_WayPoint_Unique",IsUnique = true,Order = 0)]
        public long RouteId { get; set; }
        [ForeignKey("RouteId"),ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public virtual RouteMaster fk_Route { get; set; }
        [Index("IX_WayPoint_Unique", IsUnique = true, Order = 1)]
        public long CityId { get; set; }
        public virtual CityMaster fk_City { get; set; }
        [Index("IX_WayPoint_Unique", IsUnique = true, Order = 3),Column("OrderId")]
        public int OrderId { get; set; }
        public decimal Distance { get; set; } = 0;
        public decimal TransitTime { get; set; } = 0;
        public bool PerformancePoint { get; set; } = false;
        public long? TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; } //Pickup,Drop,Passthrough & PickupDrop, ConstantType:50
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public DbGeography GeographyPoint { get; set; }    



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