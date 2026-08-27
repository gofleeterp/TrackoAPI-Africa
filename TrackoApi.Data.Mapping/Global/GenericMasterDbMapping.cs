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
    internal class GenericMasterDbMapping : EntityTypeConfiguration<GenericMaster>
    {
        public GenericMasterDbMapping()
        {
            HasRequired(x => x.fk_Form).WithMany().HasForeignKey(x=>x.FormId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_ConstantValue).WithMany().HasForeignKey(x => x.ConstantId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TaxServiceType).WithMany().HasForeignKey(x => x.TaxServiceTypeId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Ref1).WithMany().HasForeignKey(x=>x.Ref1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Ref2).WithMany().HasForeignKey(x => x.Ref2Id).WillCascadeOnDelete(false);

        }
}
}
