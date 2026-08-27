
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("tDPS")]
        public class DPS : WorkItemAuditableEntity//, IODataEntity
    {
            public DPS()
            {
                this.cDOE = DateTime.Today;
                this.TimeTaken = 0;
            }
            /*Start Date Time*/
            public DateTimeOffset SDT { get; set; }
            /*End Date Time*/
            public DateTimeOffset? EDT { get; set; }
            public double TimeTaken { get; set; } = 0;
            public long? WorkItemLogId { get; set; }
            [ForeignKey("WorkItemLogId")]
            public virtual WorkItemLog fk_WorkItemLog { get; set; }

            public long WorkItemId { get; set; }
            [ForeignKey("WorkItemId")]
            public virtual WorkItem fk_WorkItem { get; set; }
            public string Remark { get; set; }

        }
    
}
