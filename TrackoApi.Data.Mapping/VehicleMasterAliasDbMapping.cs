using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleMasterAliasDbMapping : EntityTypeConfiguration<AliasLog>
    {
        public VehicleMasterAliasDbMapping()
        {
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x=>x.TypeId).WillCascadeOnDelete(false);
        }
    }
}
