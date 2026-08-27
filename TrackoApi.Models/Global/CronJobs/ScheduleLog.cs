using CronExpressionDescriptor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global.CronJobs
{
    [Table("mScheduleLog")]
    public class ScheduleLog : AuditableEntity
    {
        public ScheduleLog()
        {
            HangfireId = Guid.NewGuid().ToString("D");
        }
        private string _cronText;

        [MaxLength(300), Index("IDX_ScheduleNameUnique", IsUnique = true)]
        public string ScheduleName { get; set; }
        /// <summary>
        /// Gets or sets the schedule type identifier.
        /// Constant Value Type Id
        /// e.g. Recurring//Once(Immediate)//Once(Delayed)//Continuations
        /// </summary>
        /// <value>The schedule type identifier.</value>
       
        public long ScheduleTypeId { get; set; }
        [ForeignKey("ScheduleTypeId")]
        public virtual ConstantValue fk_ScheduleType { get; set; }
        /// <summary>
        /// Gets or sets the interval type identifier.
        /// Constant Value Type Id
        /// e.g. Hourly//Daily//Weekly//Monthly//Quarterly//Half Yearly//Yearly
        /// </summary>
        /// <value>The schedule type identifier.</value>

        public long? IntervalTypeId { get; set; }
        [ForeignKey("IntervalTypeId")]
        public virtual ConstantValue fk_IntervalType { get; set; }

        //[Index("IDX_ScheduleUnique", IsUnique = true, Order = 3)]
        //public decimal IntervalValue { get; set; }
        public CronViewModel Cron { get; set; }
        [MaxLength(200),Index("IX_ScheduleLog_Unique",IsUnique =true)]
        public string CronText
        {
            get
            {
                return _cronText;
            }
            set {
                _cronText = value;
                try
                {
                    if (Cron == null)
                    {
                        Cron =new CronViewModel(value);
                    }
                    
                }
                catch (Exception)
                {
                    //Ignore
                }
            }
        }
        public MasterStatus Status { get; set; }
        public string CronDescription { get; set; }

        public string IsCronValid(string expression)
        {
            string message = "";
            try
            {
                CronDescription = MyCron.GetDescription(expression, true);
            }
            catch (Exception e)
            {
                this.CronText = "";
                this.CronDescription = "";
                message = e.GetBaseException().Message;
            }

            return message;
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var validationResult in ValidateLogic()) yield return validationResult;
        }

        public IEnumerable<ValidationResult> ValidateLogic()
        {
            if (!string.IsNullOrWhiteSpace(this.CronText))
            {
                string message = IsCronValid(this.CronText);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    yield return new ValidationResult(message);
                }
            }
            if (Cron != null && string.IsNullOrWhiteSpace(CronText))
            {
                this.CronText = Cron.ToString();
                string message = IsCronValid(this.CronText);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    yield return new ValidationResult(message);
                }
            }
            if (string.IsNullOrWhiteSpace(CronText))
            {
                yield return new ValidationResult("Schedule Cron Expression is Required");
            }
        }
        public string HangfireId { get; set; }
        public virtual List<JobLog> Jobs { get; set; }
    }
    
}
