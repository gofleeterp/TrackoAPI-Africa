using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

namespace TrackoApi.Models.BMS
{
    [Table("tMDOrder")]
    public class MaterialDispatchOrder:AuditableEntity
    {
        [MaxLength(200)]
        public string OrderNo {get; set; }
        public long PartId { get; set; }
        [ForeignKey("PartId")]
        public virtual MaterialMaster fk_Part { get; set; }
        public DateTime? SupplyDateTime { get; set; }
        public decimal Quantity { get; set; } = 0;
        public decimal? QuantityDispatched { get; set; } = 0;
        public decimal? QuantityRejected { get; set; } = 0;
        public long? VendorId { get; set; }
        [ForeignKey("VendorId")]
        public virtual Ledger fk_Ledger { get; set; }
        public DateTime? DeliveryAckDate { get; set; }
        [MaxLength(200)]
        public string DeliveryAckNo { get; set; }
        public long? DispatchId { get; set; }
        [ForeignKey("DispatchId")]
        public virtual VehicleMovementLog fk_Dispatch { get; set; }

        public long? ChallanId { get; set; }
        [ForeignKey("ChallanId")]
        public virtual ChallanMaster fk_ChallanId { get; set; }

        public long? ViewId { get; set; }
        [MaxLength(200)]
        public string Ref1 { get; set; }
    }
}
