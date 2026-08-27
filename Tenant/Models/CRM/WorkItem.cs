
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenant.Models.CRM
{
    [Table("tWorkItem")]
    public class WorkItem : WorkItemAuditableEntity, IValidatableObject
    {
        public WorkItem()
        {

        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(WorkItemSubject))
            {
                yield return new ValidationResult("Subject is Required", new[] { "WorkItemSubject" });
            }
            if (string.IsNullOrWhiteSpace(Particulars))
            {
                yield return new ValidationResult("Particulars is Required", new[] { "Particulars" });
            }
        }
        public string ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public virtual Application fk_Application { get; set; }
        public string TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual TenantMaster fk_Tenant { get; set; }
        /// <summary>
        /// Bug/Epic/Feature/Issue/Task/TestCase/Userstory
        /// </summary>
        public long? WorkTypeId { get; set; }
        [ForeignKey("WorkTypeId")]
        public virtual TenantConstantValue fk_WorkType { get; set; }

        [Column("WorkItemSubject"), MaxLength(2000)]
        public string WorkItemSubject { get; set; }
        public string BugTriggerPoint { get; set; }
       
        [Column("Particulars")]
        public string Particulars { get; set; }
        public long PriorityId { get; set; }
        [ForeignKey("PriorityId")]
        public virtual TenantConstantValue fk_Priority { get; set; }
        public long? WorkItemRefId { get; set; }
        [ForeignKey("WorkItemRefId")]
        public virtual WorkItem fk_WorkItemRef { get; set; }

        public long? ReleaseId { get; set; }
        [ForeignKey("ReleaseId")]
        public virtual ReleaseNote fk_Release { get; set; }
        public long? ObjectId { get; set; }
        public long? ObjectTypeId { get; set; }
        [ForeignKey("ObjectTypeId")]
        public virtual TenantConstantValue fk_ObjectType { get; set; }
        public bool IsCodeImpact { get; set; } = false;
        public long ImpactId { get; set; }
        [ForeignKey("ImpactId")]
        public virtual TenantConstantValue fk_Impact { get; set; }
        public long? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual TenantConstantValue fk_Status { get; set; }

        public long? ResolutionId { get; set; }
        [ForeignKey("ResolutionId")]
        public virtual TenantConstantValue fk_Resolution { get; set; }
        [MaxLength(200), Index("IDX_WorkItem_RefNo", IsUnique = true)]
        public string WorkItemNo { get; set; }

        public long? ViewId { get; set; }
        [ForeignKey("ViewId")]
        public virtual TenantConstantValue fk_View { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        [Column("ContactNumber"), MaxLength(100)]
        public string ContactNumber { get; set; }

        [Column("EmailAddress"), MaxLength(500)]
        public string EmailAddress { get; set; }

        //[InverseProperty("WorkItemId")]
        //public virtual List<WorkItemLog> Logs { get; set; }
        //[InverseProperty("ParentWorkItemId")]
        //public virtual List<WorkItemLog> RefLogs { get; set; }
    }
    [Table("tWDR")]
    public class WorkDeliveryReport:WorkItemAuditableEntity
    {
        public long? WorkItemId { get; set; }
        [ForeignKey("WorkItemId")]
        public virtual WorkItem fk_WorkItem { get; set; }
        /// <summary>
        /// User who co-ordinate with Customer
        /// </summary>
        public long? CoordinatorId { get; set; }
        public string BugTriggerPoint { get; set; }
        public string Consequence { get; set; }
        public string Fix { get; set; }
        public string Result { get; set; }
        public string Workaround { get; set; }
        public string Feature { get; set; }
        public string Reason { get; set; }

    }

}
