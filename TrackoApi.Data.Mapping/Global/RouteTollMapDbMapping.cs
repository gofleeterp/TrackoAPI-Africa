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
    internal class RouteTollMapDbMapping : EntityTypeConfiguration<RouteTollMap>
    {
        public RouteTollMapDbMapping()
        {
            HasRequired(x => x.fk_Route).WithMany().HasForeignKey(x=>x.RouteId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Toll).WithMany().HasForeignKey(x => x.TollId).WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_Guarantor).WithMany(x=>x.Guarantors).HasForeignKey(z=>z.GuarantorId).WillCascadeOnDelete(false);
        }
    }
}
