using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.BMS
{
    [Table("mCNBillNature")]
    //TODO:Adding to status in blank db with name "FreightBill","SupplimentaryBill"
    public class CNBillNature : AuditableEntity,IValidatableObject
    {
        [Column("Name"),MaxLength(100),Index("IDX_CNBillNature_Name",IsUnique = true,Order =1),MinLength(3)]
        public string Name { get; set; }

        [Column("Code"), MaxLength(100)]
        public string Code { get; set; }

        /// <summary>
        /// CNBillType: FreightBill & SupplimentaryBill
        /// ConstantType: 101
        /// </summary>
        [Index("IDX_CNBillNature_Name", IsUnique = true, Order = 2)]
        public long CNBillTypeId { get; set; }
        [ForeignKey("CNBillTypeId")]
        
        public virtual ConstantValue fk_BillType { get; set; }

        [Column("StatusId")]
        public MasterStatus Status { get; set; } = MasterStatus.Active;
        public bool IsReserved { get; set; } = false;
        //TODO:Add Properties for Default Ledgers

        public long? CreditAccountId { get; set; }
        [ForeignKey("CreditAccountId")]
        public virtual Ledger fk_CreditAc { get; set; }
        public long? DiscountAccountId { get; set; }
        [ForeignKey("DiscountAccountId")]
        public virtual Ledger fk_DiscountAc { get; set; }
        public long? VATAccountId { get; set; }
        [ForeignKey("VATAccountId")]
        public virtual Ledger fk_VATAc { get; set; }

        public long? IGSTAccountId { get; set; }
        [ForeignKey("IGSTAccountId")]
        public virtual Ledger fk_IGSTAc { get; set; }
        public long? CGSTAccountId { get; set; }
        [ForeignKey("CGSTAccountId")]
        public virtual Ledger fk_CGSTAc { get; set; }
        public long? SGSTAccountId { get; set; }
        [ForeignKey("SGSTAccountId")]
        public virtual Ledger fk_SGSTAc { get; set; }

        public long? OtherAccount1Id { get; set; }
        [ForeignKey("OtherAccount1Id")]
        public virtual Ledger fk_Other1Ac { get; set; }
        public long? OtherAccount2Id { get; set; }
        [ForeignKey("OtherAccount2Id")]
        public virtual Ledger fk_Other2Ac { get; set; }
        public long? OtherAccount3Id { get; set; }
        [ForeignKey("OtherAccount3Id")]
        public virtual Ledger fk_Other3Ac { get; set; }
        public long? OtherAccount4Id { get; set; }
        [ForeignKey("OtherAccount4Id")]
        public virtual Ledger fk_Other4Ac { get; set; }
        public long? OtherAccount5Id { get; set; }
        [ForeignKey("OtherAccount5Id")]
        public virtual Ledger fk_Other5Ac { get; set; }
        public long? OtherAccount6Id { get; set; }
        [ForeignKey("OtherAccount6Id")]
        public virtual Ledger fk_Other6Ac { get; set; }
        public string JsonData { get; set; } = "[]"; /*for ZRA*/

        public long? ScriptId { get; set; }
        [ForeignKey("ScriptId")]
        public virtual ApiWorkFlowScript ApiWorkFlowScript { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Bill Nature Name is Required");
            }
        }
    }
}