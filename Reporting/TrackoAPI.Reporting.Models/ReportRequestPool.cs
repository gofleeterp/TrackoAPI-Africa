using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.CronJobs;
using TrackoAPI.Models.Shared;

namespace TrackoAPI.Reporting.Models
{
    [Table("tReportRequestPool")]
    public class ReportRequestPool:AuditableEntity
    {
        public long? ReportId { get; set; }
        [ForeignKey("ReportId")]
        public virtual ApiView fk_Report { get; set; }

        public bool IsCUD { get; set; }
        public long? CustomReportId { get; set; }
        [ForeignKey("CustomReportId")]
        public virtual UserDefinedReport UserDefinedReport { get; set; }
        [Column("PrtFrmtDSId")]
        public long? PrintFormatDataSourceId { get; set; }
        public long? ProcId { get; set; }
        [ForeignKey("ProcId"), IgnoreDataMember]
        public virtual ReportProcedure fk_Proc { get; set; }
        public long? CustomProcId { get; set; }
        [ForeignKey("CustomProcId"), IgnoreDataMember]
        public virtual UserDefinedReportProcedure fk_UserDefinedProc { get; set; }
        [Column("spName"), IgnoreDataMember]
        public string Query { get; set; }

        public string Parameter1 { get; set; }
        public string Parameter2 { get; set; }
        public string Parameter3 { get; set; }
        public string Parameter4 { get; set; }
        public string Parameter5 { get; set; }
        public string Parameter6 { get; set; }
        public string Parameter7 { get; set; }
        public string Parameter8 { get; set; }
        public string Parameter9 { get; set; }
        public string Parameter10 { get; set; }
        public string Parameter11 { get; set; }
        public string Parameter12 { get; set; }
        public string Parameter13 { get; set; }
        public string Parameter14 { get; set; }
        public string Parameter15 { get; set; }
        public string Parameter16 { get; set; }
        public string Parameter17 { get; set; }
        public string Parameter18 { get; set; }
        public string Parameter19 { get; set; }
        public string Parameter20 { get; set; }
        public string Parameter21 { get; set; }
        public string Parameter22 { get; set; }
        public string Parameter23 { get; set; }
        public string Parameter24 { get; set; }
        public string Parameter25 { get; set; }
        public string Parameter26 { get; set; }
        public string Parameter27 { get; set; }
        public string Parameter28 { get; set; }
        public string Parameter29 { get; set; }
        public string Parameter30 { get; set; }
        [XmlSqlType]
        public string XmlParameter { get; set; }
        public bool IsExecuted { get; set; }
        public bool IsScheduled { get; set; } = false;
        public DocType ExportType { get; set; }
        [XmlSqlType]
        public string XmlReportSetting { get; set; }
        public double Duration { get; set; }
        public virtual List<JobLog> Jobs { get; set; }
        public object[] BuildSqlParameters()
        {
            var proc = Query;
            this.CreatedSessionId = Helper.SessionId();
            this.CreatedDOE = DateTime.Now;
            var fields = this.GetType().GetProperties();
            var list = new List<object>();

            foreach (var field in fields)
            {
                if (!proc.ToLower().Contains($"@{field.Name.ToLower()}") || proc.ToLower().Contains($"@{field.Name.ToLower()}=") || proc.ToLower().Contains($"@{field.Name.ToLower()} =")) continue;
               
                var value = field.GetValue(this, null)?.ToString();
                list.Add(string.IsNullOrWhiteSpace(value)
                    ? new SqlParameter(field.Name.ToLower(), DBNull.Value)
                    : new SqlParameter(field.Name.ToLower(), value));
            }
            return list.ToArray(); ;
        }
    }
    public class GofSqlParameter
    {
        public string CategoryName { get; set; }
        public string FromDate { get; set; }
        public string OboolParam1 { get; set; }
        public string OboolParam2 { get; set; }
        public string OboolParam3 { get; set; }
        public string ODateParam1 { get; set; }
        public string ODateParam2 { get; set; }
        public string OdecParam1 { get; set; }
        public string OdecParam2 { get; set; }
        public string OdecParam3 { get; set; }
        public string OIntParam1 { get; set; }
        public string OIntParam2 { get; set; }
        public string OIntParam3 { get; set; }
        public string OLongArrayParam1 { get; set; }
        public string OLongArrayParam2 { get; set; }
        public string OLongParam1 { get; set; }
        public string OLongParam2 { get; set; }
        public string OLongParam3 { get; set; }
        public string OStrArrayParam1 { get; set; }
        public string OStrArrayParam2 { get; set; }
        public string OStrParam1 { get; set; }
        public string OStrParam2 { get; set; }
        public string OStrParam3 { get; set; }
        public string RelatedObjects { get; set; }
        public string ReportGroup { get; set; }
        public string ReportGroup_Values { get; set; }
        public string ReportSubGroup { get; set; }
        public string ToDate { get; set; }
    }

    public class AutoCompleteItem
    {
        public long? RecordId { get; set; }
        public string Value { get; set; }
    }
}
