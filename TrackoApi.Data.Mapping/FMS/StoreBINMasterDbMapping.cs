using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.Repairs;

namespace TrackoApi.Data.Mapping
{
    internal class StoreBINMasterDbMapping : EntityTypeConfiguration<StoreBinMaster>
    {
        public StoreBINMasterDbMapping()
        {
            HasRequired(x => x.fk_Store).WithMany().HasForeignKey(x => x.StoreId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Room).WithMany().HasForeignKey(x => x.RoomId).WillCascadeOnDelete(false);
        }
    }
}