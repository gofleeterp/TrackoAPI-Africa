using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping.Global
{
    public class ReportCustomizationDbMapping:EntityTypeConfiguration<ReportCustomization>
    {
        public ReportCustomizationDbMapping()
        {
            HasRequired(x=>x.fk_Report).WithMany(x=>x.ReportCustomizations).HasForeignKey(x=>x.ReportId).WillCascadeOnDelete(true);
            ToTable("mReportCustomization");
        }
    }

    public class ReportProcedureDbMapping:EntityTypeConfiguration<ReportProcedure>
    {
        public ReportProcedureDbMapping()
        {
            Ignore(x => x.Relations).Ignore(x=>x.SchemaColumns).Property(x => x._Relations).HasColumnName("Relations");
            //Ignore(x => x._Relations);
        }
    }
}
