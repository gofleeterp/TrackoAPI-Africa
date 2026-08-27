using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base.Attributes;

namespace TrackoAPI.Reports.ViewModels.FMS
{
    public class RecentTripViewModel
    {
        public long? GPSVendorId {get;set;}
        public long? HireVehicleId {get;set;}
        public string HireVehicleNo {get;set;}
        public long? VehicleId {get;set;}
        public string VehicleNo {get;set;}
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public int ODOMeter { get; set; } = 0;
        public double KM { get; set; } = 0;
        public int TravelledKM { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double ErrorMargin { get; set; } = 1.02;
        public double MaxDiffKM { get; set; } = 32;

    }

    public class GPSTrackingResult
    {
        public long VehicleId { get; set; } = 0;
        public string Error { get; set; }
        public string Message { get; set; }
        public string VehicleNo { get; set; }
        public double KMRun { get; set; } = 0;
        public double budgetedkm { get; set; } = 0;
        public double remainingkm { get; set; } = 0;
        public double totaltravelledkm { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public double Latitude { get; set; } = 0;
        [SqlDefaultValue(DefaultValue ="0")]
        public double Angle { get; set; } = 0;
        [SqlDefaultValue(DefaultValue = "0")]
        public double Temprature { get; set; } = 0;
        public string Location { get; set; }
        public double Speed { get; set; } = 0;
        [JsonConverter(typeof(BoolConverter))]
        public bool Ignition { get; set; } = true;
        public DateTime? StatusDate { get; set; }
        public string Url { get; set; }
        public double ODOMeter { get; set; } = 0;
        public double Altitude { get; set; } = 0;
        public string TripNo { get; set; }
        public string Data1 { get; set; }
        public string Data2 { get; set; }
        public string Data3 { get; set; }
        public string Data4 { get; set; }
    }
}
