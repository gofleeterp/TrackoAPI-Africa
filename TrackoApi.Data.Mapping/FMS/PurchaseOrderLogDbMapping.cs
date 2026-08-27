using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    internal class PurchaseOrderLogDbMapping : EntityTypeConfiguration<PurchaseOrderLog>
    {
        public PurchaseOrderLogDbMapping()
        {
            
            HasRequired(x => x.fk_PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_Spare).WithMany().HasForeignKey(x => x.SpareId).WillCascadeOnDelete(false);
            // HasOptional(x => x.fk_SpareMake).WithMany().HasForeignKey(x => x.SpareMakeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Unit).WithMany().HasForeignKey(x => x.UnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TyreBrand).WithMany().HasForeignKey(x => x.TyreBrandId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BatteryBrand).WithMany().HasForeignKey(x => x.BatteryBrandId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DeliveryPlace).WithMany().HasForeignKey(x => x.DeliveryPlaceId).WillCascadeOnDelete(false);
            
            HasOptional(x => x.fk_Ref1).WithMany().HasForeignKey(x => x.Ref1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref2).WithMany().HasForeignKey(x => x.Ref2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref3).WithMany().HasForeignKey(x => x.Ref3Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref4).WithMany().HasForeignKey(x => x.Ref4Id).WillCascadeOnDelete(false);
            Ignore(x => x.DataView);
        }
    }
}
