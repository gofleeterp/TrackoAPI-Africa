using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class APLConfigDbMapping : EntityTypeConfiguration<APLConfig>
    {
        public APLConfigDbMapping()
        {
            HasOptional(x => x.fk_APLType).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Account).WithMany().HasForeignKey(x => x.AccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_VoucherType).WithMany().HasForeignKey(x => x.VoucherTypeId).WillCascadeOnDelete(false);
        }
    }
}
