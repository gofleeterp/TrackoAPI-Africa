using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping.FMS
{
    public class FleetGatePassDbMapping:EntityTypeConfiguration<FleetGatePass>
    {
        public FleetGatePassDbMapping()
        {
            HasRequired(x=>x.fk_GatePassType).WithMany().HasForeignKey(x=>x.GatePassTypeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_SenderAc).WithMany().HasForeignKey(x => x.SenderAcId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ReceiverAc).WithMany().HasForeignKey(x => x.ReceiverAcId).WillCascadeOnDelete(false);
            HasMany(x=>x.Batteries).WithOptional(x=>x.fk_GatePass).HasForeignKey(x=>x.GatePassId).WillCascadeOnDelete(false);
            HasMany(x => x.Spares).WithOptional(x => x.fk_GatePass).HasForeignKey(x => x.GatePassId).WillCascadeOnDelete(false);
            HasMany(x => x.Tyres).WithOptional(x => x.fk_GatePass).HasForeignKey(x => x.GatePassId).WillCascadeOnDelete(false);
        }
    }
}
