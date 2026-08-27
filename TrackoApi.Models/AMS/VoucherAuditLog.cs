using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.AMS
{
    [Table("mVoucherAuditLog")]
    public class VoucherAuditLog : AuditableEntity
    {
        public VoucherAuditLog()
        {
            IsAudited = false;
        }

        public DateTime AuditDate { get; set; }
        [Column("VoucherId"), ForeignKey("fk_Voucher")]
        public long VoucherId { get; set; }
        public virtual Voucher fk_Voucher { get; set; }
        [MaxLength(1000)]
        public string Remark { get; set; }
        public bool IsAudited { get; set; }
        [MaxLength(100)]
        public string Auditor { get; set; }
    }
}