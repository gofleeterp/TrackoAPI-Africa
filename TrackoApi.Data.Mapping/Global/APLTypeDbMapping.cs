using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class APLTypeDbMapping : EntityTypeConfiguration<APLType>
    {
        public APLTypeDbMapping()
        {
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
        }
    }
}
