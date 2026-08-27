using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    internal class DriverTrainingLogDbMapping : EntityTypeConfiguration<DriverTrainingLog>
    {
        public DriverTrainingLogDbMapping()
        {
            HasRequired(x => x.fk_Driver).WithMany(x=>x.TrainingLogs).HasForeignKey(x=>x.DriverId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_TrainingType).WithMany().HasForeignKey(z=>z.TrainingTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Grade).WithMany().HasForeignKey(z => z.GradeId).WillCascadeOnDelete(false);
        }
    }
}
