using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

namespace TrackoApi.Models.BMS
{
    [Table("tBillSubmission")]
    public class BillSubmission : AuditableEntity
    {
        public DateTime DocDate { get; set; }
        [MaxLength(100), StationaryCheck]
        public string DocNumber { get; set; }

        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_OfficeMaster { get; set; }

        public long? BillingPartyId { get; set; }
        [ForeignKey("BillingPartyId")]
        public virtual Ledger fk_BillingParty { get; set; }
        public int CNCount { get; set; }
        public int BillCount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPODInclosed { get; set; } = true;
        [MaxLength(500)]
        public string Remark { get; set; }
        [MaxLength(100)]
        public string SubmitedBy { get; set; }
        [MaxLength(200)]
        public string Ref1 { get; set; }
        [MaxLength(500)]
        public string Ref2 { get; set; }

        public virtual List<CNBill> Bills { get; set; }
    }
}
