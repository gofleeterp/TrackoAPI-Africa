using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.OData.Builder;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.DTS;

namespace TrackoApi.Models.BMS
{
    [Table("tCNDTSStatus")]
    public class CNDTSStatus:AuditableEntity
    {
        [MaxLength(100),Index("IDX_CNDTSStatus_Unique",IsUnique = true)]
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public long StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual DTSStatus fk_Status { get; set; }
        public long? LocationId { get; set; } = null;
        [ForeignKey("LocationId")]
        public virtual CityMaster fk_Location { get; set; }
        [MaxLength(500),DataType(DataType.MultilineText)]
        public string Remark { get; set; }
        [MaxLength(300)]
        public string Ref1 { get; set; }
        [MaxLength(300)]
        public string Ref2 { get; set; }

        public int PODCount { get; set; } = 0;
        public DateTime? Date1 { get; set; } = null;
        public long? OfficeId1 { get; set; } = null;
        [ForeignKey("OfficeId1")]
        public virtual OfficeMaster fk_Office1 { get; set; }
        public long? OfficeId2 { get; set; } = null;
        [ForeignKey("OfficeId2")]
        public virtual OfficeMaster fk_Office2 { get; set; }

        public virtual List<CNDTSStatusLog> Logs { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
    [Table("tCNDTSStatusLog")]
    public class CNDTSStatusLog:AuditableEntity
    {
        public long? CNDTSStatusId { get; set; }
        [ForeignKey("CNDTSStatusId"), ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public virtual CNDTSStatus fk_CNDTSStatus { get; set; }
        public long CNId { get; set; }
        [ForeignKey("CNId"), ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public CNMaster fk_CN { get; set; }
        public long StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual DTSStatus fk_Status { get; set; }
        [Column("StartDate"), Required]
        public DateTime StartDate { get; set; }
        [Column("EndDate")]
        public DateTime? EndDate { get; set; } = null;
        public long? LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual CityMaster fk_Location { get; set; }
        [Column("ConsumedMinutes")]
        public long? ConsumedMinutes { get; set; }
        [Column("Remark")]
        [MaxLength(500)]
        public string Remark { get; set; }
        public long? NextLogId { get; set; } = null;
        [ForeignKey("NextLogId")]
        public virtual CNDTSStatusLog fk_NextLog { get; set; }
        public long? PreviousLogId { get; set; } = null;
        [ForeignKey("PreviousLogId")]
        public virtual CNDTSStatusLog fk_PreviousLog { get; set; }
        public long? OfficeId1 { get; set; } = null;
        [ForeignKey("OfficeId1")]
        public virtual OfficeMaster fk_Office1 { get; set; }
        public long? OfficeId2 { get; set; } = null;
        [ForeignKey("OfficeId2")]
        public virtual OfficeMaster fk_Office2 { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        public bool IsAuto { get; set; } = false;
        public decimal Qty { get; set; } = 0;
        public long? StockLogId { get; set; }
        [ForeignKey("StockLogId")]
        public virtual CNStockLog fk_StockLog { get; set; }
    }
}
