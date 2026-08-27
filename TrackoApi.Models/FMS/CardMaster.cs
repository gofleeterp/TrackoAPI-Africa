using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mCardMaster")]
    public class CardMaster : AuditableEntity
    {
        [Column("CardTypeId"), Required, Index("IDX_mCardMaster_Unique", 1, IsUnique = true)]
        public long CardTypeId { get; set; }
        [ForeignKey("CardTypeId")]
        public virtual ConstantValue fk_CardType { get; set; }

        [Column("CardNo"),MaxLength(200), Required(AllowEmptyStrings = false),Index("IDX_mCardMaster_Unique", 2, IsUnique = true)]
        public string CardNo { get; set; }
        public string AccountNo { get; set; }
        public string VPA { get; set; }
        public string IFSC { get; set; }
        [Column("BankAcId"), Required]
        public long BankAcId { get; set; }
        [ForeignKey("BankAcId")]        
        public virtual Ledger fk_BankAc { get; set; }

        [Column("ExpiryDate")]
        public DateTime ExpiryDate { get; set; }
        public bool IsHotlisted { get; set; }
        public long? ViewId { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public virtual List<VehicleCardMapping> Mappings { get; set; }
    }
}
