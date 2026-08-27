using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoApi.Models.CRM;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class LedgerRoleDbMapping : EntityTypeConfiguration<LedgerRole>
    {
        public LedgerRoleDbMapping()
        {
            HasRequired(x => x.fk_Ledger).WithMany(x=>x.Roles).HasForeignKey(x => x.LedgerId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Role).WithMany().HasForeignKey(x => x.RoleId).WillCascadeOnDelete(false);
        }
    }
    public class ServiceUnitDbMapping: EntityTypeConfiguration<ServiceUnit>
    {
        public ServiceUnitDbMapping()
        {
            HasOptional(x => x.fk_DataSource).WithMany().HasForeignKey(x => x.DataSourceId).WillCascadeOnDelete(false);
        }
    }
    
}
