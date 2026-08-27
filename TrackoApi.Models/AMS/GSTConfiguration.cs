using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.AMS
{
    [Table("mGSTConfiguration")]
    public class GSTConfiguration : AuditableEntity
    {
        [Index("mGSTConfiguration_Unique", IsUnique = true, Order = 0)]
        [DataType(DataType.Date)]
        public DateTime EffactiveDate { get; set; }

        [Index("mGSTConfiguration_Unique",IsUnique =true, Order =1)]
        public long CompanyGSTTypeId { get; set; }
        [ForeignKey("CompanyGSTType")]
        public virtual ConstantValue fk_CompanyGSTType { get; set; }

        [Index("mGSTConfiguration_Unique", IsUnique = true, Order = 2)]
        public long LedgerGSTTypeId { get; set; }
        [ForeignKey("LedgerGSTType")]
        public virtual ConstantValue fk_LedgerGSTType { get; set; }

        [Index("mGSTConfiguration_Unique", IsUnique = true, Order = 3)]
        public long? LedgerId { get; set; }
        [ForeignKey("Ledger")]
        public virtual Ledger fk_Ledger { get; set; }

        public bool FlagType { get; set; }

        public long DefaultHSNCodeId { get; set; }
        [ForeignKey("DefaultHSNCode")]
        public virtual TaxServiceType fk_DefaultHSNCode { get; set; }

        [Index("mGSTConfiguration_Unique", IsUnique = true, Order = 4)]
        public long RelationTypeId { get; set; }
        [ForeignKey("RelationType")]
        public virtual ConstantValue fk_RelationType { get; set; }


    }
}
