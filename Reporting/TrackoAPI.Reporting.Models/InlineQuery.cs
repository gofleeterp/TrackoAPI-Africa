using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoAPI.Reporting.Models
{
    [Table("mReportInlineQuery")]
    public class InlineQuery
    {
        [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.None),MaxLength(100)]
        public string Id { get; set; }

        public long? ProcId { get; set; }
        [ForeignKey("ProcId")]
        public virtual ReportProcedure fk_Proc { get; set; }
        [Column("sqlQuery"), IgnoreDataMember]
        public string Query { get; set; }
    }
}
