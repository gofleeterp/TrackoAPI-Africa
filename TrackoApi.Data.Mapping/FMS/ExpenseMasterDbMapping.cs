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
    internal class ExpenseMasterDbMapping : EntityTypeConfiguration<ExpenseMaster>
    {
        public ExpenseMasterDbMapping()
        {
            HasOptional(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ExpenseCategory).WithMany().HasForeignKey(x => x.ExpenseCategoryId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AutoExp).WithMany().HasForeignKey(x => x.AutoExpId).WillCascadeOnDelete(false);
        }
    }
}
