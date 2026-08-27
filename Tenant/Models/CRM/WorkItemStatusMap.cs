
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("tWorkItemStatusMap")]
        public class WorkItemStatusMap : WorkItemAuditableEntity
        {
            public WorkItemStatusMap()
            {
                this.cDOE = DateTime.Today;
            }
            public long StatusId { get; set; }
            [ForeignKey("StatusId")]
            public virtual TenantConstantValue fk_Status { get; set; }

            public long NextStatusId { get; set; }
            [ForeignKey("NextStatusId")]
            public virtual TenantConstantValue fk_NextStatus { get; set; }
        }
    
}
