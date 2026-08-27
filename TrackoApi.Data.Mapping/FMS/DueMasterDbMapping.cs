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
    internal class DueMasterDbMapping : EntityTypeConfiguration<DueMaster>
    {
        public DueMasterDbMapping()
        {
            HasRequired(x => x.fk_DueType).WithMany().HasForeignKey(x=>x.DueTypeId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_DueAccount).WithMany().HasForeignKey(x=>x.DueAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_OtherAccount).WithMany().HasForeignKey(x => x.OtherAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PayableAccount).WithMany().HasForeignKey(x => x.PayableAccountId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_IGSTAccount).WithMany().HasForeignKey(x => x.IGSTAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_CGSTAccount).WithMany().HasForeignKey(x => x.CGSTAccountId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SGSTAccount).WithMany().HasForeignKey(x => x.SGSTAccountId).WillCascadeOnDelete(false);
        }
    }
}
