using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.FMS
{
    [Table("tDueInsuranceLog")]
    public class DueInsuranceLog : AuditableEntity
    {
        [Column("Id"), Key, ForeignKey("fk_DueTransaction")]
        public override long Id { get; set; }
        public DueTransactionLog fk_DueTransaction { get; set; }
        [Column("InsCompanyId"),ForeignKey("fk_InsuranceCompany")]
        public long? InsCompanyId { get; set; }
        public virtual Ledger fk_InsuranceCompany { get; set; }
        [Column("InsOfficerName")]
        [MaxLength(100)]
        public string InsOfficerName { get; set; }

        [Column("InsuredValue")]
        public decimal InsuredValue { get; set; } = 0;
        [Column("Compulsory")]
        public decimal Compulsory { get; set; } = 0;
        [Column("TPPremium")]
        public decimal TPPremium { get; set; } = 0;
        [Column("PACCount")]
        public long PACCount { get; set; } = 0;
        [Column("PACValue")]
        public decimal PACValue { get; set; } = 0;
        [Column("AgentName")]
        [MaxLength(100)]
        public string AgentName { get; set; }
        [Column("Premium")]
        public decimal Premium { get; set; } = 0;
        [Column("ImposedValue")]
        public decimal ImposedValue { get; set; } = 0;
        [Column("GVWOD")]
        public long GVWOD { get; set; } = 0;
        [Column("Discount")]
        public decimal Discount { get; set; } = 0;
        [Column("NCBPercent")]
        public decimal NCBPercent { get; set; } = 0;
        [Column("NCBAmount")]
        public decimal NCBAmount { get; set; } = 0;
        [Column("IsComprehensive")]
        public bool IsComprehensive { get; set; }

        [Column("IGSTTPPAmount")]
        public decimal IGSTTPPAmount { get; set; } = 0;
        [Column("CGSTTPPAmount")]
        public decimal CGSTTPPAmount { get; set; } = 0;
        [Column("SGSTTPPAmount")]
        public decimal SGSTTPPAmount { get; set; } = 0;

        [Column("IGSTTPPAmountP")]
        public decimal IGSTTPPAmountP { get; set; } = 0;
        [Column("CGSTTPPAmountP")]
        public decimal CGSTTPPAmountP { get; set; } = 0;
        [Column("SGSTTPPAmountP")]
        public decimal SGSTTPPAmountP { get; set; } = 0;

        [Column("IGSTPAmount")]
        public decimal IGSTPAmount { get; set; } = 0;
        [Column("CGSTPAmount")]
        public decimal CGSTPAmount { get; set; } = 0;
        [Column("SGSTPAmount")]
        public decimal SGSTPAmount { get; set; } = 0;

        [Column("IGSTPAmountP")]
        public decimal IGSTPAmountP { get; set; } = 0;
        [Column("CGSTPAmountP")]
        public decimal CGSTPAmountP { get; set; } = 0;
        [Column("SGSTPAmountP")]
        public decimal SGSTPAmountP { get; set; } = 0;

    }
}
