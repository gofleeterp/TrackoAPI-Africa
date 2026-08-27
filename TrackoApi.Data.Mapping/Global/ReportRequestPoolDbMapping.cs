using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoAPI.Reporting.Models;

namespace TrackoApi.Data.Mapping.Global
{
    internal class ReportRequestPoolDbMapping:EntityTypeConfiguration<ReportRequestPool>
    {
        public ReportRequestPoolDbMapping()
        {
            HasOptional(x=>x.fk_Report).WithMany().HasForeignKey(x=>x.ReportId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Proc).WithMany().HasForeignKey(x => x.ProcId).WillCascadeOnDelete(false);
            Property(x => x.XmlReportSetting).HasColumnType("xml").IsOptional();
            HasMany(x => x.Jobs).WithOptional().HasForeignKey(x => x.ReportPoolId).WillCascadeOnDelete(false);
        }
    }
}
