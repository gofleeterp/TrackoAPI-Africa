using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Global;
using TrackoAPI.Reporting.Models;

namespace TrackoApi.Data.Mapping.Reporting
{
    public class UserDefinedReportDbMapping : EntityTypeConfiguration<UserDefinedReport>
    {
        public UserDefinedReportDbMapping()
        {
            //HasRequired(x=>x.fk_ReportProcedure).WithMany().HasForeignKey(x=>x.ReportProcedureId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_ParentReport).WithMany().HasForeignKey(x=>x.ParentReportId).WillCascadeOnDelete(false);
            HasMany(x => x.Parameters).WithRequired(x => x.fk_Report).WillCascadeOnDelete(true);
            
        }
    }
}
