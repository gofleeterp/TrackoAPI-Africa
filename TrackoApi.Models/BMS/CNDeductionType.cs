using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.BMS
{
    [Table("mPaymentDeduction")]
    public class PaymentDeductionType : AuditableEntity
    {
        [MaxLength(300)]
        public string TypeName { get; set; }
        [MaxLength(300)]
        public string Code { get; set; }
        public long LedgerId { get; set; }
        [ForeignKey("LedgerId")]
        public virtual Ledger fk_Ledger { get; set; }
    }
}
