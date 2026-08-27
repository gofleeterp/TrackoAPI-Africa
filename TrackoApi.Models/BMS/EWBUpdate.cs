using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.BMS
{
    [Table("tEWBUpdateLog")]
    public class EWBUpdateLog : AuditableEntity
    {
        [MaxLength(500)]
        public string EWBNo { get; set; }
        [MaxLength(50)]
        public string VehicleNo { get; set; }
        public long? VehicleId { get; set; }
        public long? HireVehicleId { get; set; }
        public decimal RemainingDistance { get; set; }
        [MaxLength(200)]
        public string CurrentLocation { get; set; }
        [MaxLength(10)]
        public string CurrentStateCode { get; set; }
        [MaxLength(50)]
        public string CurrentPINCode { get; set; }
        [MaxLength(1000)]
        public string Reason { get; set; }
        public string JsonData { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public bool IsProceeded { get; set; }
        public DateTime? ProcessTime { get; set; }
        [MaxLength(300)]
        public string HttpRequestId { get; set; }
        public DateTime? UpDateTime { get; set; }
        public DateTime? NewExpiryTime { get; set; }
    }
}