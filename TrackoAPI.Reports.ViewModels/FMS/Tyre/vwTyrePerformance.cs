using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.Reports.ViewModels.FMS.Tyre
{
    public class vwTyreLifePerformanceBrandwiseAnalysis
    {
        public long TyreId { get; set; }
        public string TyreNo { get; set; }
        public int? TyreLife { get; set; }
        public string BrandName { get; set; }
        public string Manufacturer { get; set; }
        public string SupplierName { get; set; }
        public decimal? TyreCost { get; set; }
        public decimal? TyreTPCost { get; set; }
        public decimal? TyreScrapCost { get; set; }
        public decimal? TyreNetCost { get; set; }
        public decimal? TyreKmRun { get; set; }
        public decimal? TyreUsedMonth { get; set; }
        public decimal? TyreCPKM { get; set; }
        
        public DateTime? TyreScrapDate { get; set; }

    }
    public class vwTyrePerformanceVehiclewise
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public DateTime OnDate { get; set; }
        public DateTime OutDate { get; set; }
        public long? TyreId { get; set; }
        public string TyreNo { get; set; }
        public string BrandName { get; set; }
        public decimal? TotalTyreCost { get; set; }
        public decimal? TotalMileage { get; set; }
        public decimal? LifeCPKM { get; set; }
        public decimal? KmRun { get; set; }
        public decimal? CPKM { get; set; }
        public bool Tyrestpny { get; set; }
        public int? TyreLife { get; set; }

    }
    public class vwTyreVehicleMileageSummary
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public string VehicleNo { get; set; }

        public int? SingleIssueCount { get; set; }
        public int? MultipleIssueCount { get; set; }
        public int? STPNYIssueCount { get; set; }
        public int? SingleIssueMileage { get; set; }
        public int? MultipleIssueMileage { get; set; }

        public decimal? CalcTyreCost { get; set; }
        public decimal? AvgMileagePerTyre { get; set; }
        public decimal? AvgCostPerTyre { get; set; }
    }
    public class vwVehicleTyreModelCount
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleModel { get; set; }
        public int? BNoOfTyres { get; set; }
        public int? BNoOfStpny { get; set; }

        public int? ANoOfTyres { get; set; }
        public int? ANoOfStpny { get; set; }
        public int? StpnyDiff { get; set; }
        public int? TyreDiff { get; set; }
    }

    public class vwTyreSAWithMovementDetails //SA: Scrap Analysis
    {
        public string TyreNo { get; set; }
        public string BrandName { get; set; }
        public int? MaxLife { get; set; }
        public decimal? TyreCost { get; set; }
        public DateTime? ScrapDate { get; set; }
        public decimal? ScrapAmount { get; set; }
        public int? TotalMonthUsed { get; set; }
        public long? TotalMileage { get; set; }
        public decimal? NetCost { get; set; }
        public decimal? CPKM { get; set; }
    }
    public class vwTyreSAWithMovementSummary //SA: Scrap Analysis
    {
        public string TyrePattern { get; set; }
        public int? Qty { get; set; }
        public decimal? TyreCost { get; set; }
        public decimal? ScrapAmount { get; set; }
        public int? TotalMonthUsed { get; set; }
        public long? TotalMileage { get; set; }
        public decimal? AvgCost { get; set; }
        public decimal? AvgScrapCost { get; set; }
        public long? AvgMileage { get; set; }
        public decimal? AvgNetCost { get; set; }
        public decimal? CPKM { get; set; }

    }
    public class vwTyreSAwithLifeSpanDetail //SA : Scrap Analysis
    {
        public string TyreNo { get; set; }

        public string BrandName { get; set; }
        public int? MaxLife { get; set; }
        public decimal? TyreCost { get; set; }
        public DateTime? ScrapDate { get; set; }
        public decimal? ScrapAmount { get; set; }
        public int? TotalMonthUsed { get; set; }
        public long? LM0 { get; set; }
        public long? LM1 { get; set; }
        public long? LM2 { get; set; }
        public long? LM3 { get; set; }
        public long? LM4 { get; set; }
        public long? LM5 { get; set; }
        public long? TotalMileage { get; set; }
        public decimal? NetCost { get; set; }
        public decimal? CPKM { get; set; }
    }
    public class vwTyreSAwithLifeSpanSummary //SA : Scrap Analysis
    {
        public int? TyreLife { get; set; }
        public int? TyreCount { get; set; }
        public decimal? AvgTyreCost { get; set; }
        public decimal? AvgScrapCost { get; set; }
        public decimal? AvgNetCost { get; set; }
        public decimal? AvgMileage { get; set; }
        public decimal? CPKM { get; set; }
        public decimal? TyrePcnt { get; set; }

    }
    public class vwTyreSABrandwiseSummary //SA : Scrap Analysis
    {
        public string BrandName { get; set; }
        public int? TyreCount { get; set; }
        public decimal? TyreCost { get; set; }
        public decimal? ScrapAmount { get; set; }
        public long? LM0 { get; set; }
        public long? LM1 { get; set; }
        public long? LM2 { get; set; }
        public long? LM3 { get; set; }
        public long? LM4 { get; set; }
        public long? LM5 { get; set; }
        public long? TotalMileage { get; set; }
        public decimal? NetCost { get; set; }
        public decimal? CPKM { get; set; }

    }
    public class vwTyreExpectedLife
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string TyreNo { get; set; }
        public string BrandName { get; set; }
        public string WP { get; set; }
        public decimal? BdgtdNSD { get; set; }
        public decimal? CurNSD { get; set; }
        public decimal? TyreErosion { get; set; }
        public long? TotalMileage { get; set; }
        public decimal? EMLPerMM { get; set; }//Erosion per MM
        public long? ProjectedKM { get; set; }
    }

    public class vwRunningTyreTreadwearStatus
    {
        public string CategoryName { get; set; }
        public string ClassName { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string TyreNo { get; set; }
        public string BrandName { get; set; }
        public DateTime? OnDate { get; set; }
        public long? OnKm { get; set; }
        public string WP { get; set; }
        public DateTime? WPDate { get; set; }
        public decimal? NSD { get; set; }
        public long? KmReading { get; set; }
        public int? TyreLife { get; set; }
        public string R { get; set; }
        public string S { get; set; }
    }

    public class vwTyreStockLedgerNew
    {
        public string StoreName { get; set; }
        public string RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string Type { get; set; }
        public string Particulars { get; set; }
        public long? InQty { get; set; }
        public long? OutQty { get; set; }
        public long? DiffQty { get; set; }
        public decimal? StockValue { get; set; }
        public int? SortOrderId { get; set; }

    }

    public class vwTyreStockLedgerNewSummary
    {
        public string StoreName { get; set; }
        public long? OpQty { get; set; }
        public long? InQty { get; set; }
        public long? OutQty { get; set; }
        public long? Closing { get; set; }
        public decimal? StockValue { get; set; }

    }
}
