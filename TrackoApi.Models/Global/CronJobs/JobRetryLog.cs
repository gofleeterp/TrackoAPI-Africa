using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global.CronJobs
{
    [Table("tJobRetryLog")]
    public class JobRetryLog:Entity
    {
        public long JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public virtual JobLog fk_Job { get; set; }
        public DateTime ExecutionStartTime { get; set; }
        public DateTime? ExecutionEndTime { get; set; }
        public JobResult LastJobStatus { get; set; }        
        public string JobResponse { get; set; }
        /// <summary>
        /// Time in millisecond consumed by this job
        /// </summary>
        public long Duration { get; set; }
        public void AppendResponse(string response, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!string.IsNullOrWhiteSpace(JobResponse)) {
                JobResponse += "\n";
            }
            else
            {
                JobResponse = "";
            }
            JobResponse += $"Message:{response}, MemberName:{memberName}, LineNo:{sourceLineNumber}, FilePath:{sourceFilePath}";
        }
    }
    
}
