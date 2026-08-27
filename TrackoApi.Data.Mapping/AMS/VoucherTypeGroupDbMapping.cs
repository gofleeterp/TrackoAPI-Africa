using System.Data.Entity.ModelConfiguration;
using System.Security.Cryptography.X509Certificates;
using TrackoApi.Models.AMS;

namespace TrackoApi.Data.Mapping
{
    public class VoucherTypeGroupDbMapping:EntityTypeConfiguration<VoucherTypeGroupMapping>
    {
        public VoucherTypeGroupDbMapping()
        {
            HasOptional(x=>x.fk_Group).WithMany().HasForeignKey(x=>x.GroupId).WillCascadeOnDelete(true);
            HasOptional(x=>x.fk_VoucherType).WithMany(x=>x.GroupMappings).HasForeignKey(x=>x.VoucherTypeId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Type).WithMany(x=>x.Mappings).HasForeignKey(x=>x.TypeId).WillCascadeOnDelete(false);
        }
    }

    public class ViewFieldDbMapping : EntityTypeConfiguration<ViewField>
    {
        public ViewFieldDbMapping()
        {
            Property(x => x.DefaultRoleId).IsOptional();
            HasMany(x=>x.Mappings).WithRequired(x=>x.fk_Type).HasForeignKey(x=>x.TypeId).WillCascadeOnDelete(false);
            HasMany(x=>x.BookMaps).WithRequired(x=>x.fk_Field).HasForeignKey(x=>x.FieldId).WillCascadeOnDelete(true);
        }
    }

    public class VoucherTypeDbMapping : EntityTypeConfiguration<VoucherType>
    {
        public VoucherTypeDbMapping()
        {
           // Map(x => x.Requires("VDRequired").HasValue(2));
        }
    }
}
