using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    internal class CnChallanDbMapping : EntityTypeConfiguration<CnChallan>
    {
        public CnChallanDbMapping()
        {
            HasRequired(x => x.fk_CNMaster).WithMany().HasForeignKey(x => x.CNId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Challan).WithMany().HasForeignKey(x => x.ChallanId).WillCascadeOnDelete(true);
            HasOptional(x=>x.fk_CnChallanCnCharges).WithRequired(x=>x.fk_CnChallan).WillCascadeOnDelete(true);
            HasOptional(x=>x.fk_Triplog).WithMany(x=>x.ChallanCNs).HasForeignKey(x=>x.TriplogId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_RefStockLog).WithMany().HasForeignKey(x=>x.RefStockId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DeliveryType).WithMany().HasForeignKey(x => x.DeliveryTypeId).WillCascadeOnDelete(false);
            Ignore(x => x.tempCNStockMMLogs);
            Property(x => x._cnMMXml).HasColumnName("MMXml");
            Property(x => x.ArrivalViewId).IsOptional();
        }
    }
    internal class CnChallanChargesDbMapping : EntityTypeConfiguration<CnChallanCharges>
    {
        public CnChallanChargesDbMapping()
        {
            HasRequired(x => x.fk_CNMaster).WithMany().HasForeignKey(x => x.CNId).WillCascadeOnDelete(false);
            HasKey(x => x.Id);
        }
    }
}