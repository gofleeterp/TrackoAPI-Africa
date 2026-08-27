using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    internal class BrandMasterDbMapping : EntityTypeConfiguration<BrandMaster>
    {
        public BrandMasterDbMapping()
        {
            HasOptional(x=>x.fk_BrandNature).WithMany().HasForeignKey(x=>x.NatureId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Manufacturer).WithMany().HasForeignKey(x => x.ManufacturerId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_PlyRating).WithMany().HasForeignKey(x=>x.PlyRatingId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Pattern).WithMany().HasForeignKey(x => x.PatternId).WillCascadeOnDelete(false);
        }
    }
}