using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.FMS
{
    public class PartyRouteTimeDbMapping : EntityTypeConfiguration<PartyRouteTime>
    {
        public PartyRouteTimeDbMapping()
        {
            HasRequired(x => x.fk_Party).WithMany().HasForeignKey(x => x.PartyId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Route).WithMany().HasForeignKey(x => x.RouteId).WillCascadeOnDelete(true);
        }
    }
}
