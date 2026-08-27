using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class DriverRelativeDbMapping : EntityTypeConfiguration<DriverRelative>
    {
        public DriverRelativeDbMapping()
        {
            HasRequired(x => x.fk_RelationType).WithMany().HasForeignKey(x=>x.RelationTypeId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Gender).WithMany().HasForeignKey(x => x.GenderId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Driver).WithMany(x=>x.Relatives).HasForeignKey(x=>x.DriverId).WillCascadeOnDelete(true);
        }
    }
}
