using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    [Table("ApiViews")]
    public class ApiView:Entity
    {
        public ApiView() { }
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [MaxLength(100),Index("IX_View_ViewName",IsUnique = true)]
        public string Name { get; set; }
        [MaxLength(1)]//,Index("IX_View_ShortKey",1,IsUnique = true)]
        public string ShortKey { get; set; }
        [MaxLength(300)]
        public string DisplayText { get; set; }
        [MaxLength(200)]
        public string ToolTipText { get; set; }
        [MaxLength(200)]
        public string IconName { get; set; }
        [Column("ModuleId"),ForeignKey("ApiViewModule")]//,Index("IX_View_ShortKey", 2, IsUnique = true)]
        public long ModuleId { get; set; }
        public virtual ApiViewModule ApiViewModule { get; set; }
        [Column("ViewType")]
        public AclType EntityType { get; set; }
        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        public virtual List<ReportCustomization> ReportCustomizations { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
    }
}