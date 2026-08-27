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
    internal class TollMasterDbMapping : EntityTypeConfiguration<TollMaster>
    {
        public TollMasterDbMapping()
        {
            HasRequired(x => x.fk_City).WithMany().HasForeignKey(x=>x.CityId).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_Guarantor).WithMany(x=>x.Guarantors).HasForeignKey(z=>z.GuarantorId).WillCascadeOnDelete(false);
        }
    }
}
