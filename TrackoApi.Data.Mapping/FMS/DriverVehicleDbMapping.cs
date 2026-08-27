using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.FMS
{
    public class DriverVehicleDbMapping:EntityTypeConfiguration<DriverVehicleMapping>
    {
        public DriverVehicleDbMapping()
        {
            HasOptional(x => x.fk_NextLog).WithMany().HasForeignKey(x => x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PreviousLog).WithMany().HasForeignKey(x => x.PreviousLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DriverRole).WithMany().HasForeignKey(x => x.DriverRoleId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_DriverStatus).WithMany().HasForeignKey(x => x.DriverStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_VTSLog).WithMany().HasForeignKey(x => x.VTSLogId).WillCascadeOnDelete(false);
        }
    }
}
