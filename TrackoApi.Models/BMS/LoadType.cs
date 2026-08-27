using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.BMS
{
    [Table("mLoadType")]
    public class LoadType : AuditableEntity
    {
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Code { get; set; }
        /// <summary>
        /// constant GroupId=104
        /// </summary>
        public long? RateCriteriaId { get; set; }
        [ForeignKey("RateCriteriaId")]
        public virtual ConstantValue fk_RateCriteria { get; set; }

        public long? ScriptId { get; set; }
        [ForeignKey("ScriptId")]
        public virtual ApiWorkFlowScript ApiWorkFlowScript { get; set; }
    }
}