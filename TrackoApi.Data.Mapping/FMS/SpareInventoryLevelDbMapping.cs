using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.Repairs;

namespace TrackoApi.Data.Mapping
{
    internal class SpareInventoryLevelDbMapping : EntityTypeConfiguration<SpareInventoryLevel>
    {
        public SpareInventoryLevelDbMapping()
        {
            HasRequired(x => x.fk_SpareItem).WithMany().HasForeignKey(x => x.SpareItemId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Store).WithMany().HasForeignKey(x => x.StoreId).WillCascadeOnDelete(false);
                
        }
    }
}