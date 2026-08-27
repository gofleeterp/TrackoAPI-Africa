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
    internal class SpareAliasDbMapping : EntityTypeConfiguration<MasterAlias>
    {
        public SpareAliasDbMapping()
        {
            HasRequired(x => x.fk_ExternalApp).WithMany().HasForeignKey(x=>x.ExtAppId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_RelatedType).WithMany().HasForeignKey(x => x.RelatedTypeId).WillCascadeOnDelete(false);
        }
    }
}
