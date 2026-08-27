using System;

namespace TrackoAPI.Reports.ViewModels.FMS.Driver
{
    public class vwDriverTripPerformanceSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? DriverId { get; set; }
        public string DriverName { get; set; }
        public long? TSCount { get; set; }
        public long? TLCount { get; set; }

        public long? Days { get; set; }

        public long? TotKmRun { get; set; }
        public decimal? TotFreight { get; set; }
        public decimal? TotAdv { get; set; }
        public decimal? TotExp { get; set; }
        public decimal? TotDiff { get; set; }

        public decimal? TotFuelExp { get; set; }
        public decimal? TotBdgtFuelQty { get; set; }
        public decimal? TotActualFuelQty { get; set; }
        public decimal? TotExtraFuelQty { get; set; }
       
        public decimal? FNet { get; set; }
        public decimal? FPerday { get; set; }
        public decimal? FPerkm { get; set; }
    }
    public class vwDriverTripPerformanceDetail
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? DriverId { get; set; }
        public string DriverName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SettledDate { get; set; }
        public string TripNo { get; set; }

        public string RouteName { get; set; }
        public long? TLCount { get; set; }

        public long? Days { get; set; }

        public long? TotKmRun { get; set; }
        public decimal? Freight { get; set; }
        public decimal? TripAdv { get; set; }
        public decimal? TripExp { get; set; }
        public decimal? Diff { get; set; }

        public decimal? FuelExp { get; set; }
        public decimal? BdgtFuelQty { get; set; }
        public decimal? ActualFuelQty { get; set; }
        public decimal? ExtraFuelQty { get; set; }

        public decimal? FNet { get; set; }
        public decimal? FPerday { get; set; }
        public decimal? FPerkm { get; set; }
    }
}
