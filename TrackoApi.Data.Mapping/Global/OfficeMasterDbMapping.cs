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
    internal class OfficeMasterDbMapping : EntityTypeConfiguration<OfficeMaster>
    {
        public OfficeMasterDbMapping()
        {
            HasOptional(x => x.fk_City).WithMany().HasForeignKey(x=>x.CityId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_StateCode).WithMany().HasForeignKey(x => x.StateId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GSTR1Periodicity).WithMany().HasForeignKey(x => x.GSTR1PeriodicityId).WillCascadeOnDelete(false);
        }
    }
}
