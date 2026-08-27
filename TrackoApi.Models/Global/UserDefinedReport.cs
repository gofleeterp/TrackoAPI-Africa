using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mUserDefinedReport")]
    public class UserDefinedReport:AuditableEntity
    {
        public UserDefinedReport()
        {
            Parameters = new List<UserDefinedReportParameter>();
        }
        [MaxLength(100),Index(IsUnique =true)]
        public string Name { get; set; }
        public long? UserId { get; set; }
        public long ParentReportId { get; set; }
        [ForeignKey("ParentReportId")]
        public virtual ApiView fk_ParentReport { get; set; }
        //public long ReportProcedureId { get; set; }
        //[ForeignKey("ReportProcedureId")]
        //public virtual UserDefinedReportProcedure fk_ReportProcedure { get; set; }
        [MaxLength(1000)]
        public string GroupingColumns { get; set; }
        [MaxLength(1000)]
        public string HiddenColumns { get; set; }
        [MaxLength(1000)]
        public string FilteredColumns { get; set; }
        [MaxLength(1000)]
        [Column("TotalSumCoulmn")]
        public string SummarizedColumns { get; set; }
        [MaxLength(1000)]
        [Column("AvgSumCoulm")]
        public string AvgColumns { get; set; }
        [MaxLength(1000)]
        [Column("CounSumCoulmn")]
        public string CountColumns { get; set; }
        [MaxLength(1000)]
        public string FreezeColumn { get; set; }
        [MaxLength(1000)]
        public string FreezeRow { get; set; }
        [MaxLength(4000)]
        [Column("CndFormating")]
        public string ConditionalFormatting { get; set; }
        [MaxLength(4000)]
        [Column("StackedHeader")]
        public string StackedHeader { get; set; }
        [MaxLength(4000)]
        public string CalculatedFields { get; set; }
        public bool AllowShorting { get; set; }
        public bool AllowGrouping { get; set; }
        [Column("ColOrder")]
        public string ColumnOrder { get; set; }
        [Column("ColAlias")]
        public string ColumnAlias { get; set; }
        public virtual List<UserDefinedReportParameter> Parameters { get; set; } 
    }
}
