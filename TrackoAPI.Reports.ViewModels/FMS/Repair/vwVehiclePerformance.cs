using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.Reports.ViewModels.FMS.Repair
{
    public class vwVehiclePerformanceSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? TSCount { get; set; }
        public long? TLCount { get; set; }

        public long? Days { get; set; }

        public long? TotKmRun { get; set; }
        public decimal? TotFreight { get; set; }

        public decimal? cNet { get; set; }
        public decimal? cPerday { get; set; }
        public decimal? cPerkm { get; set; }
        public decimal? TotExp { get; set; }

        public decimal? TotTyreExp { get; set; }
        public decimal? TotRepairExp { get; set; }
        public decimal? TotDuesExp { get; set; }
        public decimal? TotDriverExp { get; set; }
        public decimal? TotGenExp { get; set; }
        public decimal? TotEMIExp { get; set; }
        public decimal? FNet { get; set; }
        public decimal? FPerday { get; set; }
        public decimal? FPerkm { get; set; }

    }
    public class vwVehicleMonthlyPerformanceSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public int TripYear { get; set; }
        public int TripMonth { get; set; }
        public long? TSCount { get; set; }
        public long? TLCount { get; set; }

        public long? Days { get; set; }

        public long? TotKmRun { get; set; }
        public decimal? TotFreight { get; set; }

        public decimal? cNet { get; set; }
        public decimal? cPerday { get; set; }
        public decimal? cPerkm { get; set; }
        public decimal? TotExp { get; set; }

        public decimal? TotTyreExp { get; set; }
        public decimal? TotRepairExp { get; set; }
        public decimal? TotDuesExp { get; set; }
        public decimal? TotDriverExp { get; set; }
        public decimal? TotGenExp { get; set; }
        public decimal? TotEMIExp { get; set; }
        public decimal? FNet { get; set; }
        public decimal? FPerday { get; set; }
        public decimal? FPerkm { get; set; }

    }
    public class vwVehicleTripPerformanceDetail
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
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
        public decimal? TotFreight { get; set; }

        public decimal? cNet { get; set; }
        public decimal? cPerday { get; set; }
        public decimal? cPerkm { get; set; }
        public decimal? TotExp { get; set; }

        public decimal? TotTyreExp { get; set; }
        public decimal? TotRepairExp { get; set; }
        public decimal? TotDuesExp { get; set; }
        public decimal? TotDriverExp { get; set; }
        public decimal? TotGenExp { get; set; }
        public decimal? TotEMIExp { get; set; }
        public decimal? FNet { get; set; }
        public decimal? FPerday { get; set; }
        public decimal? FPerkm { get; set; }

    }
    public class vwVehicleTripMileageMatrix
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }

        public long? VehicleId { get; set; }
        public int? TripYear { get; set; }
        public string VehicleNo { get; set; }

        public int? DJan { get; set; }
        public long? KJan { get; set; }

        public long? DFeb { get; set; }
        public long? KFeb { get; set; }

        public long? DMar { get; set; }
        public long? KMar { get; set; }

        public long? DApr { get; set; }
        public long? KApr { get; set; }

        public long? DMay { get; set; }
        public long? KMay { get; set; }

        public long? DJun { get; set; }
        public long? KJun { get; set; }

        public long? DJul { get; set; }
        public long? KJul { get; set; }

        public long? DAug { get; set; }
        public long? KAug { get; set; }

        public long? DSep { get; set; }
        public long? KSep { get; set; }

        public long? DOct { get; set; }
        public long? KOct { get; set; }

        public long? DNov { get; set; }
        public long? KNov { get; set; }

        public long? DDec { get; set; }
        public long? KDec { get; set; }

        public long? Dtotal { get; set; }
        public long? KTotal { get; set; }
        public long? KmPerDay { get; set; }
    }
    public class vwVehicleJobgroupRepairSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public long? TripYear { get; set; }

        public decimal? Lubes { get; set; }
        public decimal? Body { get; set; }
        public decimal? Engine { get; set; }
        public decimal? Gear { get; set; }
        public decimal? General { get; set; }
        public decimal? Electrical { get; set; }
        public decimal? Clutch { get; set; }
        public decimal? Hub { get; set; }
        public decimal? Brake { get; set; }
        public decimal? Kamani { get; set; }
        public decimal? Pump { get; set; }
        public decimal? Accessory { get; set; }
        public decimal? Crown { get; set; }
        public decimal? CenterJoint { get; set; }
        public decimal? Cooling { get; set; }
        public decimal? Steering { get; set; }
        public decimal? Others { get; set; }
        public decimal? TotalAmount { get; set; }
        public long? TotalKm { get; set; }
    }
    public class vwVehicleMonthRepairSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public long? TripYear { get; set; }

        public decimal? Jan { get; set; }
        public decimal? Feb { get; set; }
        public decimal? Mar { get; set; }
        public decimal? Apr { get; set; }
        public decimal? May { get; set; }
        public decimal? Jun { get; set; }
        public decimal? Jul { get; set; }
        public decimal? Aug { get; set; }
        public decimal? Sep { get; set; }
        public decimal? Oct { get; set; }
        public decimal? Nov { get; set; }
        public decimal? Dec { get; set; }
        public decimal? TotalAmount { get; set; }
        public long? TotalKm { get; set; }
    }
    public class vwVehicleJobtypeSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleType { get; set; }
        public int? TripYear { get; set; }
        public decimal? TotSpareAmount { get; set; }
        public decimal? TotLabourAmount { get; set; }

        public decimal? TotGeneralAmount { get; set; }
        public decimal? TotCapitalAmount { get; set; }
        public decimal? TotClaimAmount { get; set; }
        public decimal? TotAccidentAmount { get; set; }
        public decimal? TotOtherAmount { get; set; }

        public decimal? TotAmount { get; set; }
        public decimal? TotKm { get; set; }

    }
    public class vwVehicleTriplogPerformanceDetail
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TriplogNo { get; set; }
        public string RouteName { get; set; }
        public long? Days { get; set; }
        public long? TotKmRun { get; set; }
        public decimal? TotAdv { get; set; }
        public decimal? TotExp { get; set; }
        public decimal? AdvExpVariance { get; set; }
        public decimal? TotFreight { get; set; }

        public decimal? cNet { get; set; }
        public decimal? cPerday { get; set; }
        public decimal? cPerkm { get; set; }


        public decimal? TotTyreExp { get; set; }
        public decimal? TotRepairExp { get; set; }
        public decimal? TotDuesExp { get; set; }
        public decimal? TotDriverExp { get; set; }
        public decimal? TotGenExp { get; set; }
        public decimal? TotEMIExp { get; set; }
        public decimal? FNet { get; set; }
        public decimal? FPerday { get; set; }
        public decimal? FPerkm { get; set; }

    }
    public class vwVehicleTriplogPerformanceSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? TLCount { get; set; }

        public long? Days { get; set; }

        public long? TotKmRun { get; set; }
        public decimal? TotAdv { get; set; }
        public decimal? TotExp { get; set; }
        public decimal? AdvExpDiff { get; set; }

        public decimal? TotFreight { get; set; }

        public decimal? cNet { get; set; }
        public decimal? cPerday { get; set; }
        public decimal? cPerkm { get; set; }


        public decimal? TotTyreExp { get; set; }
        public decimal? TotRepairExp { get; set; }
        public decimal? TotDuesExp { get; set; }
        public decimal? TotDriverExp { get; set; }
        public decimal? TotGenExp { get; set; }
        public decimal? TotEMIExp { get; set; }
        public decimal? FNet { get; set; }
        public decimal? FPerday { get; set; }
        public decimal? FPerkm { get; set; }

    }
    public class vwVehicleTripExpBreakupDetail
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? TLCount { get; set; }
        public long? Days { get; set; }
        public long? TotKmRun { get; set; }
        public string TripNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string RouteName { get; set; }
        public string DriverName { get; set; }
        public decimal? FixExp { get; set; }
        public decimal? TollTax { get; set; }
        public decimal? Diesel { get; set; }
        public decimal? Salary { get; set; }
        public decimal? Fooding { get; set; }
        public decimal? Welfare { get; set; }
        public decimal? Entry { get; set; }
        public decimal? Phone { get; set; }
        public decimal? Challan { get; set; }
        public decimal? OverLd { get; set; }
        public decimal? Repair { get; set; }
        public decimal? Others { get; set; }
        public decimal? Total { get; set; }
    }
    public class vwVehicleTripExpBreakupSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? TLCount { get; set; }
        public long? TSCount { get; set; }
        public long? Days { get; set; }

        public long? TotKmRun { get; set; }
       
        public decimal? FixExp { get; set; }
        public decimal? TollTax { get; set; }
        public decimal? Diesel { get; set; }
        public decimal? Salary { get; set; }
        public decimal? Fooding { get; set; }
        public decimal? Welfare { get; set; }
        public decimal? Entry { get; set; }
        public decimal? Phone { get; set; }
        public decimal? Challan { get; set; }
        public decimal? OverLd { get; set; }
        public decimal? Repair { get; set; }
        public decimal? Others { get; set; }
        public decimal? Total { get; set; }
    }
}
