using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global.DTS;

namespace TrackoApi.Data.Mapping.Global.DTS
{
    public class DTSStatusMappingDbMapping : EntityTypeConfiguration<DTSStatusMapping>
    {
        public DTSStatusMappingDbMapping()
        {
            HasRequired(x => x.fk_CurrentStatus).WithMany(x=>x.StatusMappings).HasForeignKey(x => x.CurrentStatusId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_NextStatus).WithMany().HasForeignKey(x => x.NextStatusId).WillCascadeOnDelete(false);
        }
    }
}
