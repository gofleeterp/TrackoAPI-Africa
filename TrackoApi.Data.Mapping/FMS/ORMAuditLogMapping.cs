using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class ORMAuditLogDbMapping : EntityTypeConfiguration<ORMAuditLog>
    {
        public ORMAuditLogDbMapping()
        {
            HasRequired(x => x.fk_ORM).WithMany().HasForeignKey(x => x.ORMlogId).WillCascadeOnDelete(false);
        }
    }
}
