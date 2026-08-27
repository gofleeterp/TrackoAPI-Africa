using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Base;
using TrackoApi.Models.Base.Attributes;
using TrackoApi.Models.BMS;

namespace TrackoApi.Models.FMS.GPS
{
    [Table("tGPSStatusLog")]
    public class GPSStatusLog:Entity,IValidatableObject
    {
        public GPSStatusLog()
        {

        }
        public DateTime GPSTime { get; set; } = DateTime.Now;
        public string VehicleNo { get; set; }
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }
        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }
        [MaxLength(1000)]
        public string GPSLocation { get; set; }
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public double Altitude { get; set; } = 0;
        public double FuelLevel { get; set; } = 0;
        public string TrafficStatus { get; set; } = "Normal";
        public DbGeography GeographyPoint { get; protected internal set; }
        public bool IgnitionStatus { get; set; } = true;
        public int Speed { get; set; } = 0;
        [SqlDefaultValue(DefaultValue = "0")]
        public double? Angle { get; set; } = 0;
        [SqlDefaultValue(DefaultValue = "0")]
        public double? Temprature { get; set; } = 0;
        public long BudgetedKM { get; set; } = 0;
        public long TravelledKM { get; set; } = 0;
        public long RemainingKM { get; set; } = 0;
        public double KM { get; set; } = 0;
        public long? VTSId { get; set; }
        //[ForeignKey("VTSId")]
        //public virtual VTSStatusLog fk_VTSStatusLog { get; set; }
        public long? TripLogId { get; set; }
        //[ForeignKey("TripLogId")]
        //public virtual VehicleMovementLog fk_TripLog { get; set; }
        public DateTime CDOE { get; set; } = DateTime.Now;
        public double ODOMeter { get; set; } = 0;
        [SqlDefaultValue(DefaultValue ="0")]
        public double? GofKM { get; set; } = 0;
        [MaxLength(200)]
        public string TripLogNo { get; set; }
        public string Data1 { get; set; }
        public string Data2 { get; set; }
        public string Data3 { get; set; }
        public string Data4 { get; set; }
        public bool ComputeGeography()
        {
            bool isInvalid = false;
            if (Latitude > 0 || Longitude > 0 && GeographyPoint == null)
            {
                try
                {
                    GeographyPoint = DbGeography.FromText($"POINT({Latitude} {Longitude})", 4326);
                }
                catch (Exception ex)
                {
                    isInvalid = true;
                }
            }
            return isInvalid;
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(Latitude>0 || Longitude > 0&& GeographyPoint==null)
            {
                if (ComputeGeography())
                {
                    yield return new ValidationResult("Unable to parse GPS Co-Ordinate to Geography Point", new[] { "Latitude", "Longitude" });
                }
               
            }
            if(string.IsNullOrWhiteSpace(VehicleNo) && VehicleId.GetValueOrDefault() <= 0 && HireVehicleId.GetValueOrDefault() <= 0)
            {
                yield return new ValidationResult("Either VehicleNo or VehicleId or HireVehicleId are required.", new[] { "VehicleNo", "VehicleId", "HireVehicleId" });
            }
        }
        /* private static double Radians(double val)
         {
             return _toRad * val;
         }
         //private const double _radiusEarthMiles = 3959;
         private const double _radiusEarthKM = 6371;
         //private const double _m2km = 1.60934;
         private const double _toRad = Math.PI / 180;
         // cos(d) = sin(φА)·sin(φB) + cos(φА)·cos(φB)·cos(λА − λB),
         //  where φА, φB are latitudes and λА, λB are longitudes
         // Distance = d * R
         public static double DistanceBetween(double lon1, double lat1, double lon2, double lat2)
         {
             try
             {
                 if (lat1 == lat2 && lon1 == lon2)
                 {
                     return 0;
                 }
                 double sLat1 = Math.Sin(Radians(lat1));
                 double sLat2 = Math.Sin(Radians(lat2));
                 double cLat1 = Math.Cos(Radians(lat1));
                 double cLat2 = Math.Cos(Radians(lat2));
                 double cLon = Math.Cos(Radians(lon1) - Radians(lon2));

                 double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

                 double d = Math.Acos(cosD);

                 double dist = _radiusEarthKM * d;

                 return Math.Round(double.IsNaN(dist)?0:dist,4);
             }
             catch
             {
                 return (double)0;
             }
         }*/
        public static double CalculateDiff(double lat1, double long1, double lat2, double long2,double errorMargin= 1.02, double maxdiff=32)
        {
            if (errorMargin <= 0) errorMargin = 1.02;
            if (maxdiff <= 0) maxdiff = 32;
            double distance = 0;
            if ((lat1 == lat2 && long1 == long2)||lat1==0||long1==0||lat2==0||long2==0)
            {
                distance = 0;
            }
            else
            {
                const double p = 0.017453292519943295; // Math.PI / 180
                double a = 0.5 - Math.Cos((lat2 - lat1) * p) / 2 +
                           Math.Cos(lat1 * p) * Math.Cos(lat2 * p) *
                               (1 - Math.Cos((long2 - long1) * p)) / 2;

                double diff= 12742 * Math.Asin(Math.Sqrt(a)); // 2 * R; R = 6371 km
                if (diff == 0) return diff;
                if (diff > maxdiff) return maxdiff;
                distance = Math.Round(diff * errorMargin, 4);
            }
            return distance;
        }

      
        public long? GPSVendorId { get; set; }
    }
}
