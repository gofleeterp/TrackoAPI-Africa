using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class DriverPaymentDbMapping:EntityTypeConfiguration<DriverPayment>
    {
        public DriverPaymentDbMapping()
        {
            HasRequired(x=>x.fk_DrAccount).WithMany().HasForeignKey(x=>x.DrAccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_CrAccount).WithMany().HasForeignKey(x => x.CrAccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Driver).WithMany(x=>x.Payments).HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_VoucherType).WithMany().HasForeignKey(x => x.VoucherTypeId).WillCascadeOnDelete(false);
        }
    }
}
