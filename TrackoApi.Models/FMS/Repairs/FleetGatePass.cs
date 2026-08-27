using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

namespace TrackoApi.Models.FMS
{
    [Table("tFleetGatePass")]
    public class FleetGatePass:AuditableEntity
    {
        [MaxLength(100), StationaryCheck]
        public string GatePassNo { get; set; }
        public DateTime GatePassDate { get; set; }
        [MaxLength(50)]
        public string VehicleNo { get; set; }
        public long? GatePassTypeId { get; set; }
        [ForeignKey("GatePassTypeId")]
        public virtual ConstantValue fk_GatePassType { get; set; }

        public long SenderAcId { get; set; }
        [ForeignKey("SenderAcId")]
        public virtual Ledger fk_SenderAc { get; set; }
        public long? ReceiverAcId { get; set; }
        [ForeignKey("ReceiverAcId")]
        public virtual Ledger fk_ReceiverAc { get; set; }
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; }
        public virtual List<SpareLog> Spares { get; set; }
        public virtual List<TyreLog> Tyres { get; set; }
        public virtual List<BatteryLog> Batteries { get; set; }
        public decimal SpareCount { get; set; } = 0;
        public decimal TyreCount { get; set; } = 0;
        public decimal BatteryCount { get; set; } = 0;
        public long? ViewId { get; set; }
    }
}
