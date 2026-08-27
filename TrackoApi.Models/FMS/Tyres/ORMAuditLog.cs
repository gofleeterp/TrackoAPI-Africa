using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("tORMAuditLog")]
    public class ORMAuditLog : AuditableEntity
    {
        public long ORMlogId { get; set; }
        [ForeignKey("ORMlogId")]
        public virtual ORMLog fk_ORM { get; set; }
        public DateTime? VerificationDate { get; set; }
        [MaxLength(100)]
        public string VerifiedBy { get; set; }
        [MaxLength(200)]
        public string VerificationRemarks { get; set; }
        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
    }
}