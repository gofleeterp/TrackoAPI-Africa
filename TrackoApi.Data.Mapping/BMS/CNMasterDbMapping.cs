using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    internal class CNMasterDbMapping:EntityTypeConfiguration<CNMaster>
    {
        public CNMasterDbMapping()
        {
            HasMany(x=>x.BillLogs).WithOptional(x=>x.fk_CN).HasForeignKey(x=>x.CNId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_CnAdvanceId).WithMany().HasForeignKey(x => x.CnAdvanceId).WillCascadeOnDelete(false);

            HasOptional(x=>x.fk_BillingParty).WithMany().HasForeignKey(x=>x.BillingPartyId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Consignee).WithMany().HasForeignKey(x => x.ConsigneeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Consignor).WithMany().HasForeignKey(x => x.ConsignorId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_ChargedWeightUnit).WithMany().HasForeignKey(x => x.ChargedWeightUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ActualWeightUnit).WithMany().HasForeignKey(x => x.ActualWeightUnitId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_ChargedQtyUnit).WithMany().HasForeignKey(x => x.ChargedQtyUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ActualQtyUnit).WithMany().HasForeignKey(x => x.ActualQtyUnitId).WillCascadeOnDelete(false);

            HasOptional(x=>x.fk_ServiceTaxPaidBy).WithMany().HasForeignKey(x=>x.ServiceTaxPaidById).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);

            HasOptional(x=>x.fk_PkgUnit).WithMany().HasForeignKey(x=>x.PkgUnitId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TransportMode).WithMany().HasForeignKey(x => x.TransportModeId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Ref1).WithMany().HasForeignKey(x => x.Ref1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref2).WithMany().HasForeignKey(x => x.Ref2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref3).WithMany().HasForeignKey(x => x.Ref3Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref4).WithMany().HasForeignKey(x => x.Ref4Id).WillCascadeOnDelete(false);
            HasMany(x => x.DTSStatusLogs).WithRequired(x => x.fk_CN).HasForeignKey(x => x.CNId).WillCascadeOnDelete(false);
            Ignore(x => x.MultiMaterialsView);
            Ignore(x => x.EWayBills);
            Ignore(x => x.JsonDataList);
            Ignore(x => x.DynamicProperties);
            HasOptional(x => x.fk_AddRemark1).WithMany().HasForeignKey(x => x.AddRemark1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AddRemark2).WithMany().HasForeignKey(x => x.AddRemark2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AddRemark3).WithMany().HasForeignKey(x => x.AddRemark3Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AddRemark4).WithMany().HasForeignKey(x => x.AddRemark4Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AddRemark5).WithMany().HasForeignKey(x => x.AddRemark5Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AddRemark6).WithMany().HasForeignKey(x => x.AddRemark6Id).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_LessRemark1).WithMany().HasForeignKey(x => x.LessRemark1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_LessRemark2).WithMany().HasForeignKey(x => x.LessRemark2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_LessRemark3).WithMany().HasForeignKey(x => x.LessRemark3Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_LessRemark4).WithMany().HasForeignKey(x => x.LessRemark4Id).WillCascadeOnDelete(false);

            // HasOptional(x => x.CNExtraInfo).WithRequired(x => x.fk_CNMaster).WillCascadeOnDelete(true);
        }
    }

    internal class CNStockDbMapping : EntityTypeConfiguration<CNStockLog>
    {
        public CNStockDbMapping()
        {
            HasRequired(x=>x.fk_CNMaster).WithMany(x=>x.StockLogs).HasForeignKey(x=>x.CNId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_LogType).WithMany().HasForeignKey(x => x.LogTypeId).WillCascadeOnDelete(false);
            HasOptional(x=>x.RefStock).WithMany(x=>x.Outwards).HasForeignKey(x=>x.RefStockId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_ChallanCN).WithMany(x=>x.CnStockLogs).HasForeignKey(x=>x.ChallanCNId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NextLog).WithMany().HasForeignKey(x => x.NextLogId).WillCascadeOnDelete(false);
        }
    }
    internal class CNStatusDbMapping : EntityTypeConfiguration<CnStatusLog>
    {
        public CNStatusDbMapping()
        {
            HasRequired(x => x.fk_CNMaster).WithMany(x=>x.CnStatusLogs).HasForeignKey(x => x.CNId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_DocType).WithMany().HasForeignKey(x => x.DocTypeId).WillCascadeOnDelete(false);

        }
    }
    internal class CNExtraInfoDbMapping : EntityTypeConfiguration<CNExtraInfo>
    {
        public CNExtraInfoDbMapping()
        {
            HasOptional(x=>x.fk_CNMaster).WithMany().HasForeignKey(x=>x.CNId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripLog).WithMany(x => x.PODDetails).HasForeignKey(x => x.TripLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CNMaster).WithMany(x => x.PODDetails).HasForeignKey(x => x.CNId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DSVoucher).WithMany().HasForeignKey(x => x.DSVoucherId).WillCascadeOnDelete(false);
            Ignore(x => x.Data);
        }
    }
}
