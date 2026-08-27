using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;

namespace TrackoApi.Data.Mapping.AMS
{
    internal class GSTConfigurationDbMapping : EntityTypeConfiguration<GSTConfiguration>
    {
        public GSTConfigurationDbMapping()
        {
            HasRequired(x => x.fk_CompanyGSTType).WithMany().HasForeignKey(x => x.CompanyGSTTypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_LedgerGSTType).WithMany().HasForeignKey(x => x.LedgerGSTTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ledger).WithMany().HasForeignKey(x => x.LedgerId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_DefaultHSNCode).WithMany().HasForeignKey(x => x.DefaultHSNCodeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_RelationType).WithMany().HasForeignKey(x => x.RelationTypeId).WillCascadeOnDelete(false);
        }
    }
}
