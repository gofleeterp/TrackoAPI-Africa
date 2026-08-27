using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class TransactionSupportLogDbMapping : EntityTypeConfiguration<TransactionSupportLog>
    {
        public TransactionSupportLogDbMapping()
        {
            HasOptional(x => x.fk_GenericI).WithMany().HasForeignKey(x => x.Generic1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GenericII).WithMany().HasForeignKey(x => x.Generic2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ConstI).WithMany().HasForeignKey(x => x.Const1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ConstII).WithMany().HasForeignKey(x => x.Const2Id).WillCascadeOnDelete(false);
        }
    }
}
