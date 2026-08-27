using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Data.Mapping
{
    public class AccountsGroupDbMapping : EntityTypeConfiguration<AccountGroup>
    {
        public AccountsGroupDbMapping()
        {
            HasOptional(x => x.fk_ParentGroup).WithMany(x => x.ChildAccountGroups).HasForeignKey(x => x.ParentGroupId).WillCascadeOnDelete(false);
        }
    }
    public class AccountsGroupChildrenDbMapping : EntityTypeConfiguration<AccountParentChild>
    {
        public AccountsGroupChildrenDbMapping()
        {
            HasKey(x => x.RowID);
            ToTable("vwAccountGroupChild");
        }
    }
    
}
