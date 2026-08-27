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
    public class VehicleCardMappingDbMapping : EntityTypeConfiguration<VehicleCardMapping>
    {
        public VehicleCardMappingDbMapping()
        {
            HasRequired(x => x.fk_Card).WithMany(x=>x.Mappings).HasForeignKey(x => x.CardId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Driver).WithMany().HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
        }
    }
}
