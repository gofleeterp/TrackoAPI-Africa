using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class CNRateContractDbMapping : EntityTypeConfiguration<CNRateContract>
    {
        public CNRateContractDbMapping()
        {
            HasMany(x => x.RateContractLogs).WithRequired(x => x.fk_RateContract).HasForeignKey(x => x.RateContractId).WillCascadeOnDelete(true);
            HasMany(x=>x.PartyContractMaps).WithRequired(x=>x.fk_Contract).HasForeignKey(x=>x.ContractId).WillCascadeOnDelete(true);
        }
    }
    public class PartyContractDbMapping : EntityTypeConfiguration<PartyContractMap>
    {
        public PartyContractDbMapping()
        {
            HasRequired(x=>x.fk_Party).WithMany().HasForeignKey(x=>x.PartyId).WillCascadeOnDelete(true);
        }
    }
}
