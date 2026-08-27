using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class LedgerOfficeDbMapping : EntityTypeConfiguration<LedgerOffice>
    {
        public LedgerOfficeDbMapping()
        {
            HasRequired(x => x.fk_Ledger).WithMany(x=>x.Offices).HasForeignKey(x => x.LedgerId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_Office).WithMany(x => x.Ledgers).HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
        }
    }
    
}
