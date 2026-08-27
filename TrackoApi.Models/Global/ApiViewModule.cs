using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    [Table("m_Modules")]
    public class ApiViewModule : Entity
    {
        public ApiViewModule()
        {
            SubModules=new List<ApiViewModule>();
            ParentModuleId = 0;
        }
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        [MaxLength(100), Index("IX_Module_ModuleName", IsUnique = true,Order = 0)]
        public string ModuleName { get; set; }
        [MaxLength(1), Index("IX_Module_ShortKey", 1, IsUnique = true)]
        public string ShortKey { get; set; }
        [MaxLength(200)]
        public string ToolTipText { get; set; }
        [MaxLength(200)]
        public string DisplayText { get; set; }
        [ForeignKey("ParentApiViewModule"), Index("IX_Module_ShortKey", 2, IsUnique = true), Index("IX_Module_ModuleName", IsUnique = true, Order = 1)]
        public long? ParentModuleId { get; set; }
        public virtual ApiViewModule ParentApiViewModule { get; set; }
        public virtual List<ApiViewModule> SubModules { get; set; }
        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        public virtual List<ApiView> Views { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsFormModule { get; set; }
    }
}