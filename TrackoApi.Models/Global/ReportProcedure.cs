using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mReportProcedure")]
    public class ReportProcedure:Entity
    {

        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [Column("spName")]
        [MaxLength(400),IgnoreDataMember]
        public string StoredProcedureName { get; set; }
        public bool IsCUD { get; set; }
        public long ReportId { get; set; }
        [ForeignKey("ReportId")]
        public virtual ApiView fk_Report { get; set; }
        [Column("PrintFormatDSId")]
        public long? PrintFormatDataSourceId { get; set; }
        [ForeignKey("PrintFormatDataSourceId")]
        public virtual PrintFormatDataSource PrintFormatDataSource { get; set; }
        [Column("Count")]
        public long UsaseCount { get; set; }
        [IgnoreDataMember]
        public string Columns { get; set; }
        public List<SchemaColumn> SchemaColumns
        {
            get { return string.IsNullOrWhiteSpace(Columns) ? new List<SchemaColumn>() : JsonConvert.DeserializeObject<List<SchemaColumn>>(Columns); }
            set
            {
                Columns = value==null? null : JsonConvert.SerializeObject(value);
            }
        }
        /// <summary>
        /// Whether Specified Procedure Return Data as Json Or Not
        /// </summary>
        public bool IsJson { get; set; }
        [IgnoreDataMember]
        public string _Relations { get; set; }
        public List<ReportRelation> Relations
        {
            get { return string.IsNullOrWhiteSpace(_Relations) ? new List<ReportRelation>() : JsonConvert.DeserializeObject<List<ReportRelation>>(_Relations); }
            set => _Relations = value==null?null: JsonConvert.SerializeObject(value);
        }
    }
    [EdmComplexType]
    public class SchemaColumn
    {
        public string Name { get; set; }
        public string ClrType { get; set; }
        public bool AllowNull { get; set; }
    }
    [EdmComplexType]
    public class ReportRelation
    {
        public long ProcId { get; set; }
        public long ReportId { get; set; }
        public string RelationName { get; set; }
        public List<RelationMapping> MappingColumns { get; set; }
        public List<ParamaterMapping> Paramaters { get; set; }

    }
    [EdmComplexType]
    public class ParamaterMapping
    {
        public ParameterSource Source { get; set; }
        public string ParamName { get; set; }
        public string ValueSource { get; set; }
    }
    [EdmEnumType]
    public enum ParameterSource
    {
        ParentRecord,
        ReportParam,
        HardCoded
    }
    [EdmComplexType]
    public class RelationMapping
    {
        public string Left { get; set; }
        public string Right { get; set; }
    }
}