using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.FMS
{
    public class DriverNextStatusDbMapping:EntityTypeConfiguration<DriverNextStatusMapping>
    {
        public DriverNextStatusDbMapping()
        {
            HasRequired(x => x.fk_CurrentStatus).WithMany().HasForeignKey(x => x.CurrentStatusId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_NextStatus).WithMany().HasForeignKey(x => x.NextStatusId).WillCascadeOnDelete(false);
        }
    }
}
