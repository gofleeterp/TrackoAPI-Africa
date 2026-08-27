using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    internal class CNBillLogDbMapping : EntityTypeConfiguration<CNBillLog>
    {
        public CNBillLogDbMapping()
        {
            HasOptional(x => x.fk_IGSTAC).WithMany().HasForeignKey(x => x.IGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAC).WithMany().HasForeignKey(x => x.SGSTACId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAC).WithMany().HasForeignKey(x => x.CGSTACId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_SalesLog).WithMany(x=>x.BillLogs).HasForeignKey(x=>x.SalesLogId).WillCascadeOnDelete(false);
        }
    }
}
