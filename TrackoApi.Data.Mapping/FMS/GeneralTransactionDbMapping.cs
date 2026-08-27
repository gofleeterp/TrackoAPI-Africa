using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.FMS
{
    public class GeneralTransactionDbMapping : EntityTypeConfiguration<GeneralTransaction>
    {
        public GeneralTransactionDbMapping()
        {
            Ignore(x => x.Data);
            HasMany(x => x.Logs).WithOptional(x => x.fk_GenTran).HasForeignKey(x => x.GenTranId);
        }
    }
    public class GeneralTransactionLogDbMapping : EntityTypeConfiguration<GeneralTransLog>
    {
        public GeneralTransactionLogDbMapping()
        {
            Ignore(x => x.Data);
        }
    }
}
