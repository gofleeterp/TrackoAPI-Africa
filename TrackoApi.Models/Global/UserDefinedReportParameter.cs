using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    [Table("mUserDefinedReportParams")]

    public class UserDefinedReportParameter : AuditableEntity
    {
        [Index("IDX_mReportParamMap_Unique", IsUnique = true, Order = 1)]
        public long ReportId { get; set; }
        [ForeignKey("ReportId")]
        public virtual UserDefinedReport fk_Report { get; set; }

        [Index("IDX_mReportParamMap_Unique", IsUnique = true, Order = 2), Required]
        public long ParameterId { get; set; }
        [ForeignKey("ParameterId")]
        public virtual ConstantValue fk_Parameter { get; set; }
        /// <summary>
        ///AutoComplete=1,
        ///ListBox=2,
        ///Integer=3,
        ///Decimal=4,
        ///String=5,
        ///DateTime=6,
        /// </summary>
        public ReportParameterType FieldTypeId { get; set; }

        [Column("ParamCaption"), MaxLength(15)]
        public string ParameterCaption { get; set; }

        public long? EnumTypeId { get; set; }
        /// <summary>
        /// RoleTypeId: This field includes a list RoleTypes as defined ion CategoryObject Mapping form
        /// </summary>
        [Column("RoleIds"), MaxLength(50)]
        public string RoleIds { get; set; }
        [Column("RoleTypeId")]
        public long? RoleTypeId { get; set; }
        /// <summary>
        /// Gets or sets the FK_ role type identifier.
        /// ConstantTypeId 89
        /// </summary>
        /// <value>The FK_ role type identifier.</value>
        [ForeignKey("RoleTypeId")]
        public virtual ConstantValue fk_RoleTypeId { get; set; }

        public bool IsRequired { get; set; }
        [MaxLength(200)]
        public string CustomDataSource { get; set; }
        [MaxLength(500)]
        public string ProcParamName { get; set; }
    }
}