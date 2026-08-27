using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Models.Base;

namespace Tenant.Models
{
    [Table("tReportRequestPool")]
    public class TenantReportRequestPool: Entity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
        public long? ProcId { get; set; }
        [ForeignKey("ProcId"), IgnoreDataMember]
        public virtual TenantReportProcedure fk_Proc { get; set; }
        public bool IsCUD { get; set; }
        [Column("spName"), IgnoreDataMember]
        public string Query { get; set; }
        public bool IsExecuted { get; set; }
        public bool IsScheduled { get; set; } = false;
        public double Duration { get; set; }
        public string JsonProps { get; set; }
        [MaxLength(1000)]
        public string TableNameMapping { get; set; }
        [Column("CSID")]
        public long CSID { get; set; }
        [Column("CDOE")]
        public DateTime CDOE { get; set; }
        [Column("MSID")]
        public long? MSID { get; set; }
        [Column("MDOE")]
        public DateTime? mDOE { get; set; }
        public bool Debug { get; set; }

    }
    [Table("mReportProcedure")]
    public class TenantReportProcedure:Entity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        [Column("spName")]
        [MaxLength(400), IgnoreDataMember]
        public string StoredProcedureName { get; set; }
        public bool IsCUD { get; set; }
        [Column("Count")]
        public long UsageCount { get; set; }
        public bool MultipleParams { get; set; }
    }
}
