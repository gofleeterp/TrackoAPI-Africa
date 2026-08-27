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
    internal class DueInsuranceLogDbMapping : EntityTypeConfiguration<DueInsuranceLog>
    {
        public DueInsuranceLogDbMapping()
        {
            //HasRequired(x => x.).WithMany().HasForeignKey(x=>x.DriverId).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_Guarantor).WithMany(x=>x.Guarantors).HasForeignKey(z=>z.GuarantorId).WillCascadeOnDelete(false);
            HasKey(x => x.Id);
            HasRequired(x=>x.fk_DueTransaction).WithOptional(x=>x.fk_InsuranceLog).WillCascadeOnDelete(true);
        }
    }
}
