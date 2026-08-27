using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    public class ReportCustomization : Entity
    {
        [Key, Column("Id", Order = 0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        [Index("IDX_mReportParamMap_Unique", IsUnique = true, Order = 1)]
        public long ReportId { get; set; }
        [ForeignKey("ReportId")]
        public virtual ApiView fk_Report { get; set; }
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
        public string ValueFormating { get; set; }
        [MaxLength(4000)]
        [Column("CndFormating")]
        public string ConditionalFormatting { get; set; }
        [Column("ColOrderAndAlias")]
        public string ColumnOrderAndAlias { get; set; }
        [MaxLength(4000)]
        [Column("StackedHeader")]
        public string StackedHeader { get; set; }
        [MaxLength(4000)]
        public string CalculatedFields { get; set; }
        public bool AllowShorting { get; set; }
        public bool AllowGrouping { get; set; }
        public bool ScheduleAllowed { get; set; } = false;

    }
    [Table("mUserReportCustomization")]
    public class UserReportCustomization:AuditableEntity,IValidatableObject
    {
        [Index("IX_Unique_UserReportCustomization", IsUnique = true, Order = 4),MaxLength(250)]
        public string ReportName { get; set; }
        [Index("IX_Unique_UserReportCustomization",IsUnique = true,Order = 1)]
        public long? ReportId { get; set; }
        [ForeignKey("ReportId")]
        public virtual ApiView fk_Report { get; set; }

        [Index("IX_Unique_UserReportCustomization", IsUnique = true, Order = 2)]
        public long? UserDefinedReportId { get; set; }
        [ForeignKey("UserDefinedReportId")]
        public virtual UserDefinedReport UserDefinedReport { get; set; }

        [Index("IX_Unique_UserReportCustomization", IsUnique = true, Order = 3)]
        public long? UserId { get; set; }
        public bool IsDefault { get; set; } = false;
        [XmlSqlType]
        public string XmlReportSetting { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReportId.GetValueOrDefault(0) == 0 && UserDefinedReportId.GetValueOrDefault() == 0)
            {
                yield return new ValidationResult("Either Report or User Defined Report identifier are required.");
            }
        }
    }
}