using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    internal class CNMultiMaterialDbMapping:EntityTypeConfiguration<CNMultiMaterial>
    {
        public CNMultiMaterialDbMapping()
        {
            HasRequired(x=>x.fk_CN).WithMany(x=>x.Materials).HasForeignKey(x=>x.CnId).WillCascadeOnDelete(true);
            HasOptional(x=>x.fk_Material).WithMany().HasForeignKey(x=>x.MaterialId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_ActualWeightUnit).WithMany().HasForeignKey(x => x.ActualWeightUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ChargeWeightUnit).WithMany().HasForeignKey(x => x.ChargeWeightUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ChargeQtyUnit).WithMany().HasForeignKey(x => x.ChargeQtyUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ActualQtyUnit).WithMany().HasForeignKey(x => x.ActualQtyUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PkgUnit).WithMany().HasForeignKey(x => x.PkgUnitId).WillCascadeOnDelete(false);
        }
    }
}
