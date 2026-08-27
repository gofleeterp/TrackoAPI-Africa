using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class MaterialMasterDbMapping:EntityTypeConfiguration<MaterialMaster>
    {
        public MaterialMasterDbMapping()
        {
            HasRequired(x=>x.fk_MaterialGroup).WithMany(x=>x.Materials).HasForeignKey(x=>x.MaterialGroupId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Party).WithMany(x=>x.Materials).HasForeignKey(x=>x.PartyId).WillCascadeOnDelete(false);
            HasMany(x => x.LocationMappings).WithRequired(x => x.fk_Material).HasForeignKey(x => x.MaterialId).WillCascadeOnDelete(true);
        }
    }
}
