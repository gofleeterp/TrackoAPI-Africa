using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.CRM;

namespace TrackoApi.Data.Mapping.CRM
{
    public class ServiceMasterDbMapping : EntityTypeConfiguration<ServiceMaster>
    {
        public ServiceMasterDbMapping()
        {
            HasRequired(x => x.fk_Unit).WithMany().HasForeignKey(x => x.UnitId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Category).WithMany().HasForeignKey(x => x.CategoryId).WillCascadeOnDelete(false);
            Ignore(x => x.DataInfo);

        }
    }
    public class ServiceRequestDbMapping : EntityTypeConfiguration<CustomerServiceRequest>
    {
        public ServiceRequestDbMapping()
        {
            HasMany(x => x.Services).WithRequired(x => x.fk_CSR).HasForeignKey(x => x.CSRId).WillCascadeOnDelete(true);
            //Ignore(x => x.DataInfo);
            Ignore(x => x.SendInvitation);
            Ignore(x => x.Subject);
            Ignore(x => x.MailBody);
        }
    }
    public class ServiceRequestLogDbMapping : EntityTypeConfiguration<CustomerServiceRequestLog>
    {
        public ServiceRequestLogDbMapping()
        {
            //Ignore(x => x.DataInfo);
        }
    }

}
