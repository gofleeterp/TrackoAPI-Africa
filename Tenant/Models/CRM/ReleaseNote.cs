
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("tReleaseNote")]
        public class ReleaseNote : WorkItemAuditableEntity
        {
            public ReleaseNote()
            {
                this.cDOE = DateTime.Today;
            }
            public string ApplicationId { get; set; }
            [ForeignKey("ApplicationId")]
            public virtual Application fk_Application { get; set; }
            public string TenantId { get; set; }
            [ForeignKey("TenantId")]
            public virtual TenantMaster fk_Tenant { get; set; }
            public string RefNo { get; set; }
            public DateTime RefDate { get; set; }
            [MaxLength(50)]
            public string ReleaseVersion { get; set; }
        }
    
}
