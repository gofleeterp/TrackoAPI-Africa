using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class CurrencyConversionDbMapping : EntityTypeConfiguration<CurrencyConversion>
    {
        public CurrencyConversionDbMapping()
        {
            HasRequired(x => x.fk_CurType).WithMany().HasForeignKey(x => x.CurTypeId).WillCascadeOnDelete(false);
        }
    }
}
