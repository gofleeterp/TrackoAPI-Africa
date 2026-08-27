using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    class HMArrivalDbMapping: EntityTypeConfiguration<HMArrival>
    {
        public HMArrivalDbMapping()
        {
            Ignore(x => x.Data);
            HasMany(x => x.ArrivalLogs).WithRequired(x => x.fk_HMArrival).HasForeignKey(x => x.HMArrivalId).WillCascadeOnDelete(false);
        }
    }
    class HMArrivalLogDbMapping : EntityTypeConfiguration<HMArrivalLog>
    {
        public HMArrivalLogDbMapping()
        {
            Ignore(x => x.Data);            
        }
    }
}
