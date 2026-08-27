using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class UnitConverterDbMapping : EntityTypeConfiguration<UnitConverter>
    {
        public UnitConverterDbMapping()
        {
            HasRequired(x=>x.fk_FromUnit).WithMany(x=>x.UnitConversions).HasForeignKey(x=>x.FromUnitId).WillCascadeOnDelete(true);
            HasRequired(x=>x.fk_ToUnit).WithMany().HasForeignKey(x=>x.ToUnitId).WillCascadeOnDelete(false);
        }
    }
    public class SpareUnitDbMapping : EntityTypeConfiguration<SpareUnitMapping>
    {
        public SpareUnitDbMapping()
        {
            HasRequired(x=>x.fk_Spare).WithMany().HasForeignKey(x=>x.SpareId).WillCascadeOnDelete(false);
        }
    }
}
