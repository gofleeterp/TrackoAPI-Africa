using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.BMS
{
    [Table("mTaxTypeService")]
    public class TaxTypeService : AuditableEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Rate { get; set; }
        /// <summary>
        /// constantTypeid=103
        /// </summary>
        public long TaxTypeId { get; set; }
        [ForeignKey("TaxTypeId")]
        public virtual ConstantValue fk_TaxType { get; set; }

        public MasterStatus Status { get; set; }
    }
}