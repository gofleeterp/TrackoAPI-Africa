
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("tWorkItemLog")]
        public class WorkItemLog : WorkItemAuditableEntity
        {
            public WorkItemLog()
            {
                this.cDOE = DateTime.Today;
                this.TimeTaken = 0;
            }
            public double TimeTaken { get; set; }
            public bool IsRead { get; set; } = false;

            public long StatusId { get; set; }
            [ForeignKey("StatusId")]
            public virtual TenantConstantValue fk_Status { get; set; }

            public long? WorkItemId { get; set; }
            [ForeignKey("WorkItemId")]
            public virtual WorkItem fk_WorkItem { get; set; }
            
        }
    
}
