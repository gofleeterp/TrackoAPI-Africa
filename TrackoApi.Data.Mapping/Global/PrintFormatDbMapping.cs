using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping.Global
{
    public class PrintFormatMasterDbMapping:EntityTypeConfiguration<PrintFormatMaster>
    {
        public PrintFormatMasterDbMapping()
        {
            HasMany(x=>x.DataSources).WithRequired(x=>x.fk_PrintFormat).HasForeignKey(x=>x.PrintFormatId).WillCascadeOnDelete(true);
            HasRequired(x=>x.fk_View).WithMany().HasForeignKey(x=>x.ViewId).WillCascadeOnDelete(false);
        }
    }
    public class LedgerPrintFormatDbMapping : EntityTypeConfiguration<LedgerPrintFormat>
    {
        public LedgerPrintFormatDbMapping()
        {
            HasOptional(x => x.fk_AnnexureFormat).WithMany().HasForeignKey(x => x.AnnexureFormatId).WillCascadeOnDelete(false);
        }
    }
}
