using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.BMS
{
    [Table("mRateContract")]
    public class CNRateContract : AuditableEntity
    {
        [Column("Name"),MaxLength(50),Index("IX_CNRateContract_Name")]
        public string Name { get; set; }
        [MaxLength(300)]
        public string Remark { get; set; }
        public virtual List<PartyContractMap> PartyContractMaps { get; set; }
        public virtual List<CNRateContractLog> RateContractLogs { get; set; }
        public long? ViewId { get; set; }
        public long? ScriptId { get; set; }
        [ForeignKey("ScriptId")]
        public virtual ApiWorkFlowScript ApiWorkFlowScript { get; set; }
    }
    [Table("mPartyContractMap")]
    public class PartyContractMap:AuditableEntity
    {
        [Index("IX_PartyContractMap_Unique",IsUnique = true,Order = 0)]
        public long ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual CNRateContract fk_Contract { get; set; }
        [Index("IX_PartyContractMap_Unique", IsUnique = true, Order = 1)]
        public long PartyId { get; set; }

        [ForeignKey("PartyId")]
        public virtual Ledger fk_Party { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsDefault { get; set; }
    }
}