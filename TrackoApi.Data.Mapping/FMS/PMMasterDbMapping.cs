using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class PMMasterDbMapping : EntityTypeConfiguration<PMMaster>
    {
        public PMMasterDbMapping()
        {
            HasRequired(x => x.fk_Nature).WithMany().HasForeignKey(x => x.NatureId).WillCascadeOnDelete(false);
        }
    }
    public class PMScheduleDbMapping : EntityTypeConfiguration<PMSchedule>
    {
        public PMScheduleDbMapping()
        {
            //HasRequired(x => x.fk_Nature).WithMany().HasForeignKey(x => x.NatureId).WillCascadeOnDelete(false);
        }
    }
}
