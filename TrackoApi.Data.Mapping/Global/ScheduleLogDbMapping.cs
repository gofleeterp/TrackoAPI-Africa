using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global.CronJobs;

namespace TrackoApi.Data.Mapping.Global
{
    public class ScheduleLogDbMapping : EntityTypeConfiguration<ScheduleLog>
    {
        public ScheduleLogDbMapping()
        {
            //HasRequired(x => x.fk_IntervalType).WithMany().HasForeignKey(x => x.IntervalTypeId).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_ScheduleType).WithMany().HasForeignKey(x => x.ScheduleTypeId).WillCascadeOnDelete(false);
            Ignore(x => x.Cron);
            //HasMany(x => x.Jobs).WithOptional(x => x.fk_Schedule).HasForeignKey(x => x.ScheduleId).WillCascadeOnDelete(false);

        }
    }
    
}
