using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.BMS
{
    [Table("mTaxRateMaster")]
    public class TaxRateMaster : AuditableEntity
    {

        public long TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType Fk_TaxServiceType { get; set; }

        /// <summary>
        /// ConstantValue=105
        /// </summary>
        public long? EntityId { get; set; }
        [ForeignKey("EntityId")]
        public virtual ConstantValue fk_Entity { get; set; }

        public decimal Rate1 { get; set; }
        public decimal Rate2 { get; set; }
        public decimal Rate3 { get; set; }
        public decimal Rate4 { get; set; }
        public decimal Rate5 { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        public MasterStatus Status { get; set; }

        public long? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual GenericMaster fk_StateCode { get; set;}
        public long? ViewId { get; set; }
    }
}