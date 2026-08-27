using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.FMS
{
    public class VTSStatusLogDbMapping : EntityTypeConfiguration<VTSStatusLog>
    {
        public VTSStatusLogDbMapping()
        {
            //Required Columns
            HasRequired(x => x.fk_DTSStatus).WithMany().HasForeignKey(x => x.DTSStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Location).WithMany().HasForeignKey(x => x.LocationId).WillCascadeOnDelete(false);
            
            //Optional Columns
            HasOptional(x => x.fk_Driver).WithMany().HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PreviousLog).WithMany().HasForeignKey(x => x.PreviousLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GPSVendor).WithMany().HasForeignKey(x => x.GPSVendorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NextLog).WithMany().HasForeignKey(x => x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Supervisor).WithMany().HasForeignKey(x => x.SupervisorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_HireVehicle).WithMany().HasForeignKey(x => x.HireVehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany(x=>x.VTSLogs).HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            Ignore(x => x.Data);

        }
    }
    public class VTSStatusLogsubDbMapping : EntityTypeConfiguration<VTSStatusLogsub>
    {
        public VTSStatusLogsubDbMapping()
        {
            HasRequired(x => x.fk_VTSStatusLog).WithMany(x=>x.VTSStatusLogsub).HasForeignKey(x => x.VTSLogId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_DTSStatus).WithMany().HasForeignKey(x => x.DTSStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Location).WithMany().HasForeignKey(x => x.LocationId).WillCascadeOnDelete(false);
        }
    }
}
