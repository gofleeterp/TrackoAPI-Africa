using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class TyreRubberTypeDbMapping : EntityTypeConfiguration<TyreRubberType>
    {
        public TyreRubberTypeDbMapping()
        {
            HasRequired(x => x.fk_BrandNature).WithMany().HasForeignKey(x=>x.BrandNatureId).WillCascadeOnDelete(false);
            

        }
    }
}
