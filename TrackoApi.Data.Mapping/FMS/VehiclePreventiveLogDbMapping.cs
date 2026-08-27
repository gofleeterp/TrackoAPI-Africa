using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehiclePreventiveLogDbMapping : EntityTypeConfiguration<VehiclePreventiveLog>
    {
        public VehiclePreventiveLogDbMapping()
        {
            
            HasOptional(x => x.fk_JobCard).WithMany().HasForeignKey(x => x.JobCardId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_NextLog).WithMany().HasForeignKey(x=>x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PreviousLog).WithMany().HasForeignKey(x=>x.PreviousLogId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_PMMaster).WithMany().HasForeignKey(x=>x.PMId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NewPMMaster).WithMany().HasForeignKey(x => x.NewPMId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_PMSchedule).WithMany().HasForeignKey(x=>x.ScheduleId).WillCascadeOnDelete(false);
        }
    }
}
