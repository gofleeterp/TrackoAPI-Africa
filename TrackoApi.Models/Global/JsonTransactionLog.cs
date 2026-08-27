using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("tJsonTranLog")]
    public class JsonTransactionLog: AuditableEntity
    {
        [Key]
        public override long Id { get; set; }
        
        [Index("IXD_JsonTranLog_Key",IsUnique = true,Order = 1),Index("IXD_JsonTranLog_RecordId")]
        public long RecordId { get; set; }
        
        [Index("IXD_JsonTranLog_Key", IsUnique = true, Order = 3), Index("IXD_JsonTranLog_No"),MaxLength(100)]
        public string RecordNo { get; set; }
        public DateTime? RecordDate { get; set; }
        public long ViewId { get; set; }
        
        [MaxLength(200)]
        [Index("IXD_JsonTranLog_Key",IsUnique = true,Order = 2)]
        public string Key { get; set; }

        public string JsonData { get; set; }
        public DateTime? APRLDateTime { get; set; }
        public long? APRLCSId { get; set; }
        public long? APRLUserId { get; set; }
        public bool IsAPRLRequired { get; set; } = false;
        public int? ApprovedStatusId { get; set; } = 0;
    }
}