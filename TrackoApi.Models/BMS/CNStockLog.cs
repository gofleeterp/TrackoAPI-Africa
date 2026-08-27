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
    [Table("tCNStockLog")]
    public class CNStockLog : AuditableEntity
    {
        public CNStockLog()
        {
            StockMMLogs=new List<CNStockMMLog>();
        }
        /// <summary>
        /// Gets or sets the log date.
        /// <remarks>Transaction Date for which Log has been Created e.g. New CN Challan or Delievered</remarks>
        /// </summary>
        /// <value>The log date.</value>
        public DateTime LogDate { get; set; }
        public long CNId { get; set; }
        [ForeignKey("CNId"),ActionOnDelete(EdmOnDeleteAction.Cascade)]
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
        public decimal DamagedQty { get; set; }
        public decimal OutQty { get; set; } = 0;
        /// <summary>
        /// Gets or sets the log type identifier.
        /// <remarks>
        /// Options StockIn,StockOut,Expected and Delivered
        /// ConstantTypeId 108
        /// </remarks>
        /// </summary>
        /// <value>The log type identifier.</value>
        public long LogTypeId { get; set; }
        [ForeignKey("LogTypeId")]
        public ConstantValue fk_LogType { get; set; }
        /// <summary>
        /// Gets or sets the challan identifier.
        /// <remarks>
        /// in case LogType is "Stock Out","EnRoute","Stock In" or if required "Delivered"
        /// </remarks>
        /// </summary>
        /// <value>The challan identifier.</value>
        //public long? ChllanId { get; set; }
        //[ForeignKey("ChallanId")]
        //public virtual ChallanMaster fk_Challan { get; set; }
        public long? ChallanCNId { get; set; }
        [ForeignKey("ChallanCNId")]
        public virtual CnChallan fk_ChallanCN { get; set; }
        [Column("TriplogId"), ForeignKey("fk_Triplog")]
        public long? TriplogId { get; set; }
        public virtual VehicleMovementLog fk_Triplog { get; set; }
        public long? RefStockId { get; set; }
        [ForeignKey("RefStockId")]
        public virtual CNStockLog RefStock { get; set; }

        public List<CNStockLog> Outwards { get; set; }
        public virtual List<CNStockMMLog> StockMMLogs { get; set; }
        public CNStockLog Clone()
        {
            return (CNStockLog)this.MemberwiseClone();
        }
        [MaxLength(200)]
        public string Ref1 { get; set; }

        public long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        public virtual CNStockLog fk_NextLog { get; set; }

        public virtual List<CNDTSStatusLog> StatusLogs { get; set; }
        
    }
}
