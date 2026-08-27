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
    internal class TollRateLogDbMapping : EntityTypeConfiguration<TollRateLog>
    {
        public TollRateLogDbMapping()
        {
            HasRequired(x => x.fk_AxleType).WithMany().HasForeignKey(x=>x.VehicleAxleTypeId).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_Guarantor).WithMany(x=>x.Guarantors).HasForeignKey(z=>z.GuarantorId).WillCascadeOnDelete(false);
        }
    }
}
