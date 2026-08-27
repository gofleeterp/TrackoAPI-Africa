using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;

namespace TrackoApi.Models.FMS
{
    [Table("mRouteMaster")]
    public class RouteMaster : AuditableEntity, IValidatableObject
    {
        public RouteMaster()
        {
            this.WayPoints=new List<RouteWayPoint>();
            this.AllowedVehicleTypes = new List<RouteVehicleType>();
            this.ChildRoutes = new List<ChildParentRoute>();
            this.ParentRoutes = new List<ChildParentRoute>();
            this.Budgets = new List<TripExpenseBudget>();
            this.ClientRoutes = new List<PartyRouteTime>();
            
        }

        [Column("Name"), Required, Index("IDX_mRouteMaster_Name", IsUnique = true), MaxLength(200)]
        public string Name { get; set; }

        [Column("Abbr"), Required, Index("IDX_mRouteMaster_Abbr", IsUnique = true), MaxLength(200)]
        public string Abbr { get; set; }

        [Column("FromPlaceId"), ForeignKey("fk_FromPlace"),Required]
        public long FromPlaceId { get; set; }
        public virtual CityMaster fk_FromPlace { get; set; }

        [Column("ToPlaceId"), ForeignKey("fk_ToPlace"),Required]
        public long ToPlaceId { get; set; }
        public virtual CityMaster fk_ToPlace { get; set; }
        [Column("ClientId")]
        public long? ClientId { get; set; }
        [ForeignKey("ClientId")]
        public virtual Ledger fk_Client { get; set; }

        [Column("NatureId")]
        public long? NatureId { get; set; }
        [ForeignKey("NatureId")]
        public GenericMaster fk_NatureId { get; set; }


        [Column("TransitKm")]
        public long TransitKm { get; set; }

        [Column("TransitHours")]
        public long TransitHours { get; set; }

        public bool IsReturnRoute { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public virtual List<RouteWayPoint> WayPoints { get; set; }
        public virtual List<ChildParentRoute> ChildRoutes { get; set; }
        public virtual List<ChildParentRoute> ParentRoutes { get; set; }
        public virtual List<RouteVehicleType> AllowedVehicleTypes { get; set; }
        public virtual List<TripExpenseBudget> Budgets { get; set; }
        public virtual List<PartyRouteTime> ClientRoutes { get; set; }
        public long? ViewId { get; set; }

        [Column("GoogleKm")]
        public long GoogleKm { get; set; }
        public DateTime? ReviewDate { get; set; }
        [Column("ExtraProperties")]
        public string ExtraProperties { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(ExtraProperties)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties)): _dt;
            get
            {
                try
                {
                    if (ExtraProperties == "{}") ExtraProperties = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties ?? (ExtraProperties = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                ExtraProperties = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }
        }
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((ExtraProperties ?? "{}") == "{}") ExtraProperties = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((ExtraProperties ?? (ExtraProperties = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                ExtraProperties = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                ExtraProperties = "[]";
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TransitKm == 0)
            {
                yield return new ValidationResult($"Total Route Km is mandatory");
            }
        }
    }
}
