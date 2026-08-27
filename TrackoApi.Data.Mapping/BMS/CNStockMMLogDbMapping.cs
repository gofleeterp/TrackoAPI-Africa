
using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class CNStockMMLogDbMapping : EntityTypeConfiguration<CNStockMMLog>
    {
        public CNStockMMLogDbMapping()
        {
            HasOptional(x => x.fk_ChallanCN).WithMany(x=>x.CnMMLogs).HasForeignKey(x => x.ChallanCNId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_CNMaterial).WithMany().HasForeignKey(x => x.MaterialId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_CNMaster).WithMany().HasForeignKey(x => x.CNId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_CNMM).WithMany().HasForeignKey(x => x.CNMMId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_LogType).WithMany().HasForeignKey(x => x.LogTypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_StockLog).WithMany(x=>x.StockMMLogs).HasForeignKey(x => x.StockLogId).WillCascadeOnDelete(true);
            HasOptional(x => x.RefStock).WithMany(x => x.Outwards).HasForeignKey(x => x.RefStockId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NextLog).WithMany().HasForeignKey(x => x.NextLogId).WillCascadeOnDelete(false);
        }
    }
    public class CNStockMMLogViewDbMapping : EntityTypeConfiguration<vw_CNStockMMLog>
    {
        public CNStockMMLogViewDbMapping()
        {
            HasKey(x => x.Id);
            ToTable("vw_CNStockMMLog");
        }
    }
    public class CNStockLogViewDbMapping : EntityTypeConfiguration<vw_CNStockLog>
    {
        public CNStockLogViewDbMapping()
        {
            HasKey(x => x.Id);
            ToTable("vw_CNStockLog");
        }
    }
}
