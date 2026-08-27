using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class SpareMasterDbMapping : EntityTypeConfiguration<SpareMaster>
    {
        public SpareMasterDbMapping()
        {
            HasOptional(x => x.fk_AfterUse).WithMany().HasForeignKey(x => x.AfterUseId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SpareGroup).WithMany().HasForeignKey(x => x.SpareGroupId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SpareNature).WithMany().HasForeignKey(x => x.SpareNatureId).WillCascadeOnDelete(false);
            HasMany(x => x.Units).WithRequired(x => x.fk_Spare).HasForeignKey(x => x.SpareId).WillCascadeOnDelete(false);
            HasMany(x=>x.Aliases).WithOptional(x=>x.fk_SpareItem).HasForeignKey(x=>x.SpareItemId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_CNMaterial).WithMany().HasForeignKey(x => x.CNMaterialId).WillCascadeOnDelete(true);
        }
    }
}
