using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.BMS
{
    [Table("tCNEWayBill")]
    public class CNEWayBill:AuditableEntity
    {
        public long? ConsignorId { get; set; }
        [ForeignKey("ConsignorId")]
        public virtual Ledger fk_Consignor { get; set; }
        public long? ConsigneeId { get; set; }
        [ForeignKey("ConsigneeId")]
        public virtual Ledger fk_Consignee { get; set; }
        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }
        public string EWayBillNo { get; set; }
        public decimal KM { get; set; }
        public int PostalCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }
        [MaxLength(100)]
        public string VehicleNo { get; set; }
        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public long? CNId { get; set; }
        public long? TripLogId { get; set; }
        [MaxLength(200)]
        public string OriginLocation { get; set; }
        [MaxLength(200)]
        public string DestinationLocation { get; set; }
        public long? OriginCityId { get; set; }
        [ForeignKey("OriginCityId")]
        public virtual CityMaster OriginCity { get; set; }
        public long? DestinationCityId { get; set; }
        [ForeignKey("DestinationCityId")]
        public virtual CityMaster DestinationCity { get; set; }
        [MaxLength(100)]
        public string FromGstinNo { get; set; }
        [MaxLength(500)]
        public string FromParty { get; set; }
        [MaxLength(1000)]
        public string FromAddress { get; set; }
        [MaxLength(20)]
        public string FromPincode { get; set; }
        [MaxLength(100)]
        public string FromState { get; set; }

        [MaxLength(100)]
        public string ToGstinNo { get; set; }
        [MaxLength(500)]
        public string ToParty { get; set; }
        [MaxLength(1000)]
        public string ToAddress { get; set; }
        [MaxLength(20)]
        public string ToPincode { get; set; }
        [MaxLength(100)]
        public string ToState { get; set; }
        
        public decimal InvoiceValue { get; set; }
        [MaxLength(100)]
        public string TransporterGstin { get; set; }
        [MaxLength(500)]
        public string TransporterName { get; set; }
        public int NoOfDays { get; set; }
        public string JsonData { get; set; }
        public string VehiclListDetails { get; set; }
        public string ItemDetails { get; set; }
        [MaxLength(300)]
        public string HttpRequestId { get; set; }

    }
}