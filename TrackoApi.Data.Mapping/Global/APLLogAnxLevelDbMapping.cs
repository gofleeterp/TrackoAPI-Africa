using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class APLLogAnxLevelDbMapping : EntityTypeConfiguration<APLLogAnxLevel>
    {
        public APLLogAnxLevelDbMapping()
        {
            HasRequired(x => x.fk_APLAnx).WithMany().HasForeignKey(x => x.APLAnxId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_APLLog).WithMany().HasForeignKey(x => x.APLLogId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_APLConfig).WithMany().HasForeignKey(x => x.APLConfigId).WillCascadeOnDelete(false);            
        }
    }
}
