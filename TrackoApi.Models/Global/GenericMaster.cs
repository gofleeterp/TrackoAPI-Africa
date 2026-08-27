using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    [Table("mGenericMaster")]
    public class GenericMaster : AuditableEntity
    {
        public GenericMaster()
        {
            Status = MasterStatus.Active;
        }

        [Column("Name"), Index("IDX_mGenericMaster_Unique", 1, IsUnique = true), Required, MaxLength(200)]
        public string Name { get; set; }

        [Column("Abbr"), Required, MaxLength(200)]
        public string Abbreviation { get; set; }

        [Column("FormId"), Required,ForeignKey("fk_Form")]
        public long? FormId { get; set; }
        
        public virtual ApiView fk_Form { get; set; }
        [Column("ConstantId"), ForeignKey("fk_ConstantValue"), Index("IDX_mGenericMaster_Unique", 2, IsUnique = true)]
        public long ConstantId { get; set; }
        public virtual ConstantValue fk_ConstantValue { get; set; }

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;

        [MaxLength(100)]
        public string BatchId { get; set; }
        /// <summary>
        /// 1) Incase of State Master we would capture GST State Code
        /// </summary>
        public string Ref1 { get; set; }

        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_Ref1 { get; set; }

        public long? Ref2Id { get; set; }
        [ForeignKey("Ref2Id")]
        public virtual GenericMaster fk_Ref2 { get; set; }
        [Column("Constant1Id")]
        public long? Constant1Id { get; set; }
        [ForeignKey("Constant1Id")]
        public virtual ConstantValue fk_Constant1Value { get; set; }
    }
    
}
