using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.FMS
{
    public class GeneralExpenseLogDbMapping:EntityTypeConfiguration<GeneralExpenseLog>
    {
        public GeneralExpenseLogDbMapping()
        {
            HasRequired(x => x.fk_CreditAccount).WithMany().HasForeignKey(x => x.CreditAccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_DebitAccount).WithMany().HasForeignKey(x => x.DebitAccountId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Driver).WithMany().HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
            HasOptional(x => x.fK_Voucher).WithMany().HasForeignKey(x => x.VoucherId).WillCascadeOnDelete(true);
            HasOptional(x => x.fk_ExpenseNature).WithMany().HasForeignKey(x => x.ExpenseNatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PaidIn).WithMany().HasForeignKey(x => x.PaidInId).WillCascadeOnDelete(false);
            Ignore(x => x.GenerateVoucher);            
        }
    }
}
