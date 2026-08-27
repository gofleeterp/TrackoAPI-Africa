using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    public class ApiWorkFlowScript:AuditableEntity
    {
        [MaxLength(100)]
        public string Name  { get; set; }
        [MaxLength(4000)]
        public string Script { get; set; }
        /// <summary>
        /// Gets or sets the script type identifier.
        /// </summary>
        /// <value>The script type identifier.</value>
        public long ScriptTypeId { get; set; }
        [ForeignKey("ScriptTypeId")]
        public virtual ConstantValue fk_ScriptType { get; set; }

        public long? ViewId { get; set; }
        [ForeignKey("ViewId")]
        public virtual ApiView fk_View { get; set; }

        public MasterStatus Status { get; set; }=MasterStatus.Active;

    }
}
