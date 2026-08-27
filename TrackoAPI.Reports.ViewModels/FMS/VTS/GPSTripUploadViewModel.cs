using System;

namespace TrackoAPI.Reports.ViewModels
{
    public class GPSTripUploadViewModel
    {
        public string VehicleNo { get; set; }
        public string RegistrationNo { get; set; }        
        public DateTime TripStartDate { get; set; }
        public string Consignor { get; set; }
        public string Consignee { get; set; }
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public string TripNo { get; set; }
        public double KM { get; set; }
        public string DriverName { get; set; }
        public string DriverMobile { get; set; }
        public DateTime? ETA { get; set; }
        public string ConsigneeAddress { get; set; }
        public string ConsignoreAddress { get; set; }
        public string Remark { get; set; }
        public decimal Qty { get; set; }
        public long TripId { get; set; }
        public DateTime? LoadingDate { get; set; }
        public long Id { get; set; }
        public string TenantId { get; set; }
        public int Order { get; set; }
        public long TypeId { get; set; }
        public decimal PointKM { get; set; }
        public decimal TravalTime { get; set; }
        public decimal StopageTime { get; set; }
        public string ToCityStateName { get; set; }
        public string PostalCode { get; set; }
        public DateTime? LoadingReportDate { get; set; }
        public decimal ETAHour { get; set; }
        public DateTime? ScheduledPlacementDate { get; set; }
        public DateTime? ScheduledDepartureDate { get; set; }
        public string TripNature { get; set; }
        public string CNNos { get; set; }
        public string RouteName { get; set; }
    }
}
