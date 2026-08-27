using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    internal class CNBillMasterDbMapping : EntityTypeConfiguration<CNBill>
    {
        public CNBillMasterDbMapping()
        {
            HasMany(x => x.BillLogs).WithRequired(x => x.fk_Bill).HasForeignKey(x => x.BillId).WillCascadeOnDelete(true);

            HasRequired(x => x.fk_SalesAc).WithMany().HasForeignKey(x => x.SalesAccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_BillingPartyAc).WithMany().HasForeignKey(x => x.BillingPartyAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_DiscountAc).WithMany().HasForeignKey(x => x.DiscountAccountId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Other1Ac).WithMany().HasForeignKey(x => x.OtherAccount1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Other2Ac).WithMany().HasForeignKey(x => x.OtherAccount2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Other3Ac).WithMany().HasForeignKey(x => x.OtherAccount3Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Other4Ac).WithMany().HasForeignKey(x => x.OtherAccount4Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Other5Ac).WithMany().HasForeignKey(x => x.OtherAccount5Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Other6Ac).WithMany().HasForeignKey(x => x.OtherAccount6Id).WillCascadeOnDelete(false);

            HasOptional(x=>x.fk_RecoveryOffice).WithMany().HasForeignKey(x=>x.RecoveryOfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_BillOffice).WithMany().HasForeignKey(x => x.BillOfficeId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Voucher).WithMany().HasForeignKey(x=>x.VoucherId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_VDR).WithMany().HasForeignKey(x => x.VDRId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_CoverNote).WithMany(x=>x.Bills).HasForeignKey(x=>x.CoverNoteId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_IGSTAC).WithMany().HasForeignKey(x => x.IGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAC).WithMany().HasForeignKey(x => x.SGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAC).WithMany().HasForeignKey(x => x.CGSTACId).WillCascadeOnDelete(false);
            Ignore(x => x.JsonBillLogs);
        }
    }
}
