
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("tWorkItemRefLog")]
        public class WorkItemReferenceLog: WorkItemAuditableEntity
        {
            public WorkItemReferenceLog()
            {
                this.cDOE = DateTime.Today;
            }
            public long ParentWorkItemId { get; set; }
            [ForeignKey("ParentWorkItemId")]
            public virtual WorkItem fk_ParentWorkItem { get; set; }
            public long RefWorkItemId { get; set; }
            [ForeignKey("RefWorkItemId")]
            public virtual WorkItem fk_RefWorkItem { get; set; }
        }
    
}
