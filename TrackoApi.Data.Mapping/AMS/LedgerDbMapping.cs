using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.AMS;

namespace TrackoApi.Data.Mapping
{
    internal class LedgerDbMapping : EntityTypeConfiguration<Ledger>
    {
        public LedgerDbMapping()
        {
            //HasRequired(x => x.fk_Office).WithRequiredPrincipal().WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_BillOffice).WithOptionalPrincipal().WillCascadeOnDelete(false);
            //HasOptional(x=>x.fk_Office).WithOptionalPrincipal().WillCascadeOnDelete(false);

            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BillOffice).WithMany().HasForeignKey(x => x.BillingOfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AccountRole).WithMany().HasForeignKey(x => x.AccountRoleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CreditNature).WithMany().HasForeignKey(x => x.CreditNatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Group).WithMany(x=>x.Ledgers).HasForeignKey(x => x.GroupId).WillCascadeOnDelete(false);
            Property(x => x.FullName).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
            HasOptional(x=>x.ParentCompany).WithMany(x=>x.Subsidiaries).HasForeignKey(x=>x.ParentCompanyId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_SalesAccount).WithMany().HasForeignKey(x=>x.SalesAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_UnbilledSalesAccount).WithMany().HasForeignKey(x => x.UnbilledSalesAcId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GeoLocation).WithMany().HasForeignKey(x => x.GeoLocationId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_LoadType).WithMany().HasForeignKey(x => x.LoadTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SalesCategory).WithMany().HasForeignKey(x => x.SalesCategoryId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_Material).WithMany().HasForeignKey(x => x.MaterialId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Unit).WithMany().HasForeignKey(x => x.UnitId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_CreditCurType).WithMany().HasForeignKey(x => x.CreditCurTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BankAc1).WithMany().HasForeignKey(x => x.BankAc1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_BankAc2).WithMany().HasForeignKey(x => x.BankAc2Id).WillCascadeOnDelete(false);

            Ignore(x => x.JsonDataList);
            Ignore(x => x.DynamicProperties);
        }
    }
}
