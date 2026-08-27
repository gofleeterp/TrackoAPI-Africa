using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.BMS
{
    [Table("mTaxServiceType")]
    public class TaxServiceType : AuditableEntity
    {
        [MaxLength(4000)]
        public string Description { get; set; }
        [MaxLength(200),Index("IDX_mTaxServiceType_Unique",IsUnique = true,Order = 1)]
        public string Code { get; set; }

        /// <summary>
        /// constantTypeid=103
        /// </summary>
        [Index("IDX_mTaxServiceType_Unique", IsUnique = true, Order = 2)]
        public long TaxTypeId { get; set; }
        [ForeignKey("TaxTypeId")]
        public virtual ConstantValue fk_TaxType { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }
        public virtual List<Ledger> Ledgers { get; set; }

        /*zra link*/
        [MaxLength(25)]
        public string PortalTaxType { get; set; }

        public bool IsReserved { get; set; }
    }
}