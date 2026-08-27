using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("tTPTRequestPool")]
    public class TPTRequestPool : AuditableEntity
    {

        [Column("RecordId"), DatabaseGenerated(DatabaseGeneratedOption.None), Index("IX_TPTRequestPool_Unique", IsUnique = true, Order = 1), Required]
        public long RecordId { get; set; }

        [Column("RequestId"), Index("IX_TPTRequestPool_Unique", IsUnique = true, Order = 2), MaxLength(150), Required]
        public string RequestId { get; set; }

        [Column("ViewId"), Index("IX_TPTRequestPool_Unique", IsUnique = true, Order = 3), Required]
        public long ViewId { get; set; }
        [MaxLength(150)]
        public string TypeKey { get; set; }

        [MaxLength(150)]
        public string DocNo { get; set; }

        [MaxLength(150)]
        public string BatchId { get; set; }

        [MaxLength(500)]
        public string Ref1 { get; set; }
        [MaxLength(500)]
        public string Ref2 { get; set; }
        [MaxLength(500)]
        public string Ref3 { get; set; }
        [MaxLength(500)]
        public string Ref4 { get; set; }
        [MaxLength(150)]
        public string Status { get; set; }

        [MaxLength(50)]
        public string ResponseCode { get; set; }

        [MaxLength(500)]
        public string Remarks { get; set; }

        public long? Ref1Id { get; set; }
        public long? Ref2Id { get; set; }

        public bool IsAPLPassed { get; set; }
        public long? APLUserId { get; set; }
        public DateTime? APLDateTime { get; set; }

        public bool IsProceeded { get; set; }
        public string HttpResult { get; set; }
        public string CatchError { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime? ExecutedTime { get; set; }
    }
}