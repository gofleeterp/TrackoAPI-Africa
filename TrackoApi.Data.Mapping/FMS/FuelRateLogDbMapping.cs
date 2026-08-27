using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    internal class FuelRateLogDbMapping : EntityTypeConfiguration<FuelRateLog>
    {
        public FuelRateLogDbMapping()
        {
            HasRequired(x => x.fk_Fuel).WithMany().HasForeignKey(x => x.FuelId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Pump).WithMany().HasForeignKey(x => x.PumpId).WillCascadeOnDelete(false);
            
        }
    }
}