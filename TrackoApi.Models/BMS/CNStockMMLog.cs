using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.BMS
{
    [Table("tCNStockMMLog")]
    public class CNStockMMLog : AuditableEntity
    {
        public long StockLogId { get; set; }
        [ForeignKey("StockLogId")]
        public CNStockLog fk_StockLog { get; set; }

        public long MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public MaterialMaster fk_CNMaterial { get; set; }

        public long CNMMId { get; set; }
        [ForeignKey("CNMMId")]
        public CNMultiMaterial fk_CNMM { get; set; }
        public DateTime LogDate { get; set; }
        public long CNId { get; set; }
        [ForeignKey("CNId"), ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public CNMaster fk_CNMaster { get; set; }
        /// <summary>
        /// Gets or sets the office identifier.
        /// Transaction Office
        /// </summary>
        /// <value>The office identifier.</value>
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public OfficeMaster fk_Office { get; set; }
        public decimal InQty { get; set; } = 0;
        public decimal ShortageQty { get; set; } = 0;
        public decimal ExessQty { get; set; } = 0;
        public decimal OutQty { get; set; } = 0;
        public long LogTypeId { get; set; }
        [ForeignKey("LogTypeId")]
        public ConstantValue fk_LogType { get; set; }

        public long? ChallanCNId { get; set; }
        [ForeignKey("ChallanCNId")]
        public virtual CnChallan fk_ChallanCN { get; set; }

        public long? RefStockId { get; set; }
        [ForeignKey("RefStockId")]
        public virtual CNStockMMLog RefStock { get; set; }
        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public virtual CNStockMMLog fk_NextLog { get; set; }

        public List<CNStockMMLog> Outwards { get; set; }
        [Column("TriplogId"), ForeignKey("fk_Triplog")]
        public long? TriplogId { get; set; }
        public virtual VehicleMovementLog fk_Triplog { get; set; }
        [MaxLength(200)]
        public string DeliveryAcknowledgeNo { get; set; }

        public long? DeliveryLocationId { get; set; }
        [ForeignKey("DeliveryLocationId")]
        public virtual GenericMaster fk_DeliveryLocation { get; set; }

        public long? WarehouseLocationId { get; set; }
        [ForeignKey("WarehouseLocationId")]
        public virtual GenericMaster fk_WarehouseLocation { get; set; }


        public CNStockMMLog Clone()
        {
            return (CNStockMMLog) this.MemberwiseClone();
        }
        [MaxLength(200)]
        public string Ref1 { get; set; }
        [MaxLength(200)]
        public string Ref2 { get; set; }//DispatchInvoiceNo

        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public object DamagedQty { get; set; }
    }
}
