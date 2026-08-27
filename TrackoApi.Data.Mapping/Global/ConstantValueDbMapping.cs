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
    internal class ConstantValueDbMapping : EntityTypeConfiguration<ConstantValue>
    {
        public ConstantValueDbMapping()
        {
           HasRequired(x => x.fk_ConstantType).WithMany(x=>x.ConstantValues).HasForeignKey(x=>x.ConstantTypeId).WillCascadeOnDelete(false);
        }
    }
}
