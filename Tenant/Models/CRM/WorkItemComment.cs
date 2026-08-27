
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("tWorkItemComments")]
        public class WorkItemComment : WorkItemAuditableEntity
        {
            public WorkItemComment()
            {
                this.cDOE = DateTime.Today;
            }
            
            public long? CommentRefId { get; set; }
            [ForeignKey("CommentRefId")]
            public virtual WorkItemComment fk_CommentRef { get; set; }

            public long WorkItemId { get; set; }
            [ForeignKey("WorkItemId")]
            public virtual WorkItem fk_WorkItem { get; set; }

            public long? WorkItemLogId { get; set; }
            [ForeignKey("WorkItemLogId")]
            public virtual WorkItemLog fk_WorkItemLog { get; set; }
            [MaxLength(4000)]
            public string Remarks { get; set; }
        }
    
}
