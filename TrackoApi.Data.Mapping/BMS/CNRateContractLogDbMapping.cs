using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class CNRateContractLogDbMapping : EntityTypeConfiguration<CNRateContractLog>
    {
        public CNRateContractLogDbMapping()
        {
            HasRequired(x => x.fk_RateContract).WithMany().HasForeignKey(x => x.RateContractId).WillCascadeOnDelete(false);

            HasOptional(x=>x.fk_MaterialGroup).WithMany().HasForeignKey(x=>x.MaterialgroupId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_LoadType).WithMany().HasForeignKey(x => x.LoadTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A1Factor).WithMany().HasForeignKey(x => x.A1FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A2Factor).WithMany().HasForeignKey(x => x.A2FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A3Factor).WithMany().HasForeignKey(x => x.A3FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A4Factor).WithMany().HasForeignKey(x => x.A4FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A5Factor).WithMany().HasForeignKey(x => x.A5FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A6Factor).WithMany().HasForeignKey(x => x.A6FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A7Factor).WithMany().HasForeignKey(x => x.A7FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A8Factor).WithMany().HasForeignKey(x => x.A8FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A9Factor).WithMany().HasForeignKey(x => x.A9FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A10Factor).WithMany().HasForeignKey(x => x.A10FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A11Factor).WithMany().HasForeignKey(x => x.A11FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A12Factor).WithMany().HasForeignKey(x => x.A12FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A13Factor).WithMany().HasForeignKey(x => x.A13FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A14Factor).WithMany().HasForeignKey(x => x.A14FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A15Factor).WithMany().HasForeignKey(x => x.A15FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A16Factor).WithMany().HasForeignKey(x => x.A16FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_A17Factor).WithMany().HasForeignKey(x => x.A17FactorId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_L1Factor).WithMany().HasForeignKey(x => x.L1FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_L2Factor).WithMany().HasForeignKey(x => x.L2FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_L3Factor).WithMany().HasForeignKey(x => x.L3FactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Consignee).WithMany().HasForeignKey(x => x.ConsigneeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Consignor).WithMany().HasForeignKey(x => x.ConsignorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_State).WithMany().HasForeignKey(x => x.StateId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Material).WithMany().HasForeignKey(x => x.MaterialId).WillCascadeOnDelete(false);//added by sanjay

            HasOptional(x => x.fk_DisFactor).WithMany().HasForeignKey(x => x.DisFactorId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripMode).WithMany().HasForeignKey(x => x.TripModeId).WillCascadeOnDelete(false);
        }
    }
}
