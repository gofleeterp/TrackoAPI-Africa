using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.TMS
{
    public class TypeOfServiceDbMapping : EntityTypeConfiguration<TaxTypeService>
    {
        public TypeOfServiceDbMapping()
        {
            HasRequired(x=>x.fk_TaxType).WithMany().HasForeignKey(x=>x.TaxTypeId).WillCascadeOnDelete(false);
        }
    }
}
