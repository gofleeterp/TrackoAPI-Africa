using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    internal class ApiUserDbMapping : EntityTypeConfiguration<ApiUser>
    {
        public ApiUserDbMapping()
        {
            ToTable("ApiUsers");
            HasMany(x => x.ResourceAccessLogs).WithRequired().HasForeignKey(x => x.UserId);
            HasOptional(x => x.fk_ReportingManager).WithMany(x=>x.TeamMembers).HasForeignKey(x => x.ReportingManagerId).WillCascadeOnDelete(false);
            Property(x => x.Email).IsOptional();
            HasOptional(x=>x.fk_Office).WithMany().HasForeignKey(x=>x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Address).WithMany().HasForeignKey(x=>x.AddressId).WillCascadeOnDelete(false);
            HasMany(x=>x.IpUserMappings).WithRequired().HasForeignKey(x=>x.UserId).WillCascadeOnDelete(true);
        }
    }
    internal class PermissionSetDbMapping:EntityTypeConfiguration<PermissionSet>
    {
        public PermissionSetDbMapping()
        {
            HasTableAnnotation("IsView", "View");
            HasKey(x => x.Id);
            ToTable("View_PermissionSet");
        }
    }


}
