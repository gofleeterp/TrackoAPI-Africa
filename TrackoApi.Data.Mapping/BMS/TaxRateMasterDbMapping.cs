using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class TaxRateMasterDbMapping : EntityTypeConfiguration<TaxRateMaster>
    {
        public TaxRateMasterDbMapping()
        {
            HasOptional(x=>x.fk_Entity).WithMany().HasForeignKey(x=>x.EntityId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_StateCode).WithMany().HasForeignKey(x => x.StateId).WillCascadeOnDelete(false);
            HasRequired(x => x.Fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
        }
    }
}
