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
    public class CardMasterDbMapping : EntityTypeConfiguration<CardMaster>
    {
        public CardMasterDbMapping()
        {
            HasRequired(x => x.fk_BankAc).WithMany().HasForeignKey(x => x.BankAcId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_CardType).WithMany().HasForeignKey(x => x.CardTypeId).WillCascadeOnDelete(false);
        }
    }
}
