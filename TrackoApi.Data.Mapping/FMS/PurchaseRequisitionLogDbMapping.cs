using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    internal class PurchaseRequisitionLogDbMapping : EntityTypeConfiguration<PurchaseRequisitionLog>
    {
        public PurchaseRequisitionLogDbMapping()
        {
            HasRequired(x => x.fk_PR).WithMany(x=>x.Logs).HasForeignKey(x => x.PRId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_Spare).WithMany().HasForeignKey(x => x.SpareId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Unit).WithMany().HasForeignKey(x => x.UnitId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Status).WithMany().HasForeignKey(x => x.StatusId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Ref1).WithMany().HasForeignKey(x => x.Ref1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref2).WithMany().HasForeignKey(x => x.Ref2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref3).WithMany().HasForeignKey(x => x.Ref3Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref4).WithMany().HasForeignKey(x => x.Ref4Id).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_TyreBrand).WithMany().HasForeignKey(x => x.TyreBrandId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BatteryBrand).WithMany().HasForeignKey(x => x.BatteryBrandId).WillCascadeOnDelete(false);

            Ignore(x => x.DataView);
        }
    }
}