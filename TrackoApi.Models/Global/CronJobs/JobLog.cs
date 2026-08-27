using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global.CronJobs
{
    [Table("mJob")]
    public class JobLog:AuditableEntity
    {
        public JobLog()
        {
            //JobId = Guid.NewGuid().ToString("N");
            InlineAddresses = "";
        }
        //[MaxLength(255)]
        //public string JobId { get; set; }
        [MaxLength(300),Index("IDX_Unique",IsUnique = true)]
        public string JobName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual ScheduleLog fk_Schedule { get; set; }
        /// <summary>
        /// Gets or sets the interval type identifier.
        /// Constant Value Type Id
        /// e.g. Hourly=>1495//Daily//Weekly//Monthly//Quarterly//Half Yearly//Yearly
        /// </summary>
        /// <value>The schedule type identifier.</value>
        /// ConstantTypeId 121
        public long? IntervalTypeId { get; set; }
        [ForeignKey("IntervalTypeId")]
        public virtual ConstantValue fk_IntervalType { get; set; }
        public int IntervalValue { get; set; } = 1;
        /// <summary>
        /// Constants from ConstantTypeId in(122,124,125,126,127)
        /// Should have corresponding ConstantValue as from above selected JobCategory
        ///  1507:Alert//1508:ApiJob//1509:SqlJob
        /// </summary>
        public long? JobNatureId { get; set; }
        [ForeignKey("JobNatureId")]
        public virtual ConstantValue fk_JobNature { get; set; }
        /// <summary>
        /// Gets or sets job message type
        /// </summary>
        /// <completionlist cref="NotificationType"/>
        public NotificationType MessageType { get; set; }
        [MaxLength(500)]
        public string Subject { get; set; }
        public bool SubjectIsTemplate { get; set; } = false;
        [DataType(DataType.Text)]
        public string MessageBody { get; set; }
        public bool BodyIsTemplate { get; set; } = false;
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <example>text/plain,text/html</example>
        /// <value>The type of the content.</value>
        [MaxLength(200)]
        public string ContentType { get; set; }
        public long? ReportPoolId { get; set; }
        public bool BodyHasEmbeddedData { get; set; } = false;
        public DateTime? LastExecutionOn { get; set; }
        public JobResult LastJobStatus { get; set; }
        public int MaxRetry { get; set; }
        public virtual List<MessageAddress> MessageAddresses { get; set; }
        public virtual List<JobRetryLog> Logs { get; set; }
        /// <summary>
        /// Can be defined when Job is to intrect with ThirdParty System which need authorization header
        /// Plz don't include keyword "Authorization" as it would be internally added
        /// </summary>
        public string AuthorizationKey { get; set; }

        public JobRetryLog LogExecution()
        {
            this.LastExecutionOn = DateTime.Now;
            this.ObjectState = Models.Base.ObjectState.Modified;
            this.LastJobStatus = JobResult.Running;
            var joblog = new JobRetryLog()
            {
                ObjectState = Models.Base.ObjectState.Added,
                ExecutionStartTime = this.LastExecutionOn.GetValueOrDefault(DateTime.Now),
                fk_Job = this,
                JobId = this.Id,
                LastJobStatus = TrackoAPI.Models.Shared.JobResult.Running
            };
            if (Logs == null) Logs = new List<JobRetryLog>();
            Logs.Add(joblog);
            return joblog;
        }
        public string _ExtendedInfo { get; set; }
        public string InlineAddresses { get; set; }
    }
    
}
