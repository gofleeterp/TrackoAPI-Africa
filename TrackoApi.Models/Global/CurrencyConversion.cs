using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mCurrencyConversion")]
    public class CurrencyConversion : AuditableEntity
    {
        [Column("CurTypeId"), Index("IX_CurrencyConversion_CurType", IsUnique = true,Order =1), Required]
        public long CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }


        [Column("CurDate"), Index("IX_CurrencyConversion_CurType", IsUnique = true, Order = 2), DataType(DataType.Date), Required]
        public DateTime? CurDate { get; set; }

        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
