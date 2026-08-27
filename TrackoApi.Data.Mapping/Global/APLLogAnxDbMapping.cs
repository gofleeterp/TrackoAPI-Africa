using System.Data;
using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class APLLogAnxDbMapping : EntityTypeConfiguration<APLLogAnx>
    {
        public APLLogAnxDbMapping()
        {
            HasRequired(x => x.fk_APLLog).WithMany().HasForeignKey(x => x.APLLogId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_GenRef1).WithMany().HasForeignKey(x => x.GenRef1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GenRef2).WithMany().HasForeignKey(x => x.GenRef2Id).WillCascadeOnDelete(false);
        }
    }
}
