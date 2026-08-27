using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.BMS;

namespace TrackoApi.Data.Mapping.BMS
{
    public class CNDTSStatusLogDbMapping : EntityTypeConfiguration<CNDTSStatusLog>
    {
        public CNDTSStatusLogDbMapping()
        {
            //Required Columns
            HasOptional(x => x.fk_CNDTSStatus).WithMany(x=>x.Logs).HasForeignKey(x => x.CNDTSStatusId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Location).WithMany().HasForeignKey(x => x.LocationId).WillCascadeOnDelete(false);

            //Optional Columns
            HasOptional(x => x.fk_PreviousLog).WithMany().HasForeignKey(x => x.PreviousLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NextLog).WithMany().HasForeignKey(x => x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_StockLog).WithMany(x=>x.StatusLogs).HasForeignKey(x=>x.StockLogId).WillCascadeOnDelete(false);
        }
    }
}
