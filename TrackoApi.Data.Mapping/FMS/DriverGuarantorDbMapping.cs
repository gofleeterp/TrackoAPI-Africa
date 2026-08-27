using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class DriverGuarantorDbMapping : EntityTypeConfiguration<DriverGuarantor>
    {
        public DriverGuarantorDbMapping()
        {
            HasRequired(x => x.fk_Driver).WithMany(x => x.Guarantors).HasForeignKey(x => x.DriverId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Guarantor).WithMany().HasForeignKey(z => z.GuarantorId).WillCascadeOnDelete(false);
        }
    }
}
