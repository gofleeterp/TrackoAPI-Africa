using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global.CronJobs;

namespace TrackoApi.Data.Mapping.Global
{
    public class JobLogDbMapping : EntityTypeConfiguration<JobLog>
    {
        public JobLogDbMapping()
        {
            HasOptional(x => x.fk_JobNature).WithMany().HasForeignKey(x => x.JobNatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_IntervalType).WithMany().HasForeignKey(x => x.IntervalTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Schedule).WithMany(x=>x.Jobs).HasForeignKey(x => x.ScheduleId).WillCascadeOnDelete(false);
            HasMany(x=>x.MessageAddresses).WithRequired(x=>x.fk_Job).HasForeignKey(x=>x.JobId).WillCascadeOnDelete(true);
            HasMany(x => x.Logs).WithRequired(x => x.fk_Job).HasForeignKey(x => x.JobId).WillCascadeOnDelete(true);
        }
    }

    public class MassageAddressDbMapping : EntityTypeConfiguration<MessageAddress>
    {
        public MassageAddressDbMapping()
        {
            HasRequired(x=>x.fk_Contact).WithMany().HasForeignKey(x=>x.ContactId).WillCascadeOnDelete(true);
        }
    }
}
