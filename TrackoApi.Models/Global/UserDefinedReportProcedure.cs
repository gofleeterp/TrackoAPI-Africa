using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mCustomReportProcedure")]
    public class UserDefinedReportProcedure : AuditableEntity
    {
        [Column("spName")]
        [MaxLength(500)]
        public string StoredProcedureName { get; set; }

        public long UserDefinedReportId { get; set; }
        [ForeignKey("UserDefinedReportId")]
        public virtual UserDefinedReport fk_Report { get; set; }
        [Column("Count")]
        public long UsaseCount { get; set; }
        [MaxLength(500)]
        public string Columns { get; set; }
        public bool IsJSON { get; set; }
    }
}