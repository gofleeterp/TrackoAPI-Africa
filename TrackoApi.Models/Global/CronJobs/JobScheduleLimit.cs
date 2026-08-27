using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global.CronJobs
{
    /// <summary>
    /// Provision to Restrict Job Scheduling for respactive tenant to reduce server load
    /// </summary>
    [Table("mJobScheduleLimit")]
    public class JobScheduleLimit:Entity
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override long Id { get; set; }
        /// <summary>
        /// Gets or sets the interval type identifier.
        /// Constant Value Type Id
        /// e.g. Hourly//Daily//Weekly//Monthly//Quarterly//Half Yearly//Yearly
        /// </summary>
        /// <value>The schedule type identifier.</value>
        public long IntervalTypeId { get; set; }
        [ForeignKey("IntervalTypeId")]
        public virtual ConstantValue fk_IntervalType { get; set; }

        public int MaxJobCount { get; set; }
    }
    
}
