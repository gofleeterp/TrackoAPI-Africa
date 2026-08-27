using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class TaxServiceTypeDbMapping : EntityTypeConfiguration<TaxServiceType>
    {
        public TaxServiceTypeDbMapping()
        {
            HasRequired(x=>x.fk_TaxType).WithMany().HasForeignKey(x=>x.TaxTypeId).WillCascadeOnDelete(false);
            HasMany(x=>x.Ledgers).WithOptional(x=>x.fk_ServiceType).HasForeignKey(x=>x.ServiceTypeId).WillCascadeOnDelete(false);
        }
    }
}
