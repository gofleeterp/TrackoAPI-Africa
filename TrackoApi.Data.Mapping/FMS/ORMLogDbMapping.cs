using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class ORMLogDbMapping : EntityTypeConfiguration<ORMLog>
    {
        public ORMLogDbMapping()
        {
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_ORMOrigin).WithMany().HasForeignKey(x => x.ORMOriginId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_ORMType).WithMany().HasForeignKey(x => x.ORMTypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Place).WithMany().HasForeignKey(x => x.PlaceId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_SpareGroup).WithMany().HasForeignKey(x => x.SpareGroupId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Driver).WithMany().HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Nature).WithMany().HasForeignKey(x => x.NatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Supervisor).WithMany().HasForeignKey(x => x.SupervisorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Triplog).WithMany().HasForeignKey(x => x.TriplogId).WillCascadeOnDelete(false);
            

        }
    }
}
