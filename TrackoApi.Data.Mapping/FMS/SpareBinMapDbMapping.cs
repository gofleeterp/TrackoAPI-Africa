using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.Repairs;

namespace TrackoApi.Data.Mapping
{
    internal class SpareBinMapDbMapping : EntityTypeConfiguration<SpareBinMapping>
    {
        public SpareBinMapDbMapping()
        {
            HasRequired(x => x.fk_SpareItem).WithMany(x=>x.Bins).HasForeignKey(x => x.SpareItemId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Store).WithMany(x => x.Bins).HasForeignKey(x => x.StoreId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Bin).WithMany().HasForeignKey(x => x.BinId).WillCascadeOnDelete(false);
        }
    }
}