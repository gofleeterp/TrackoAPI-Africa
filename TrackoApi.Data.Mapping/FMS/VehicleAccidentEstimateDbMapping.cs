using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleAccidentEstimateDbMapping : EntityTypeConfiguration<VehicleAccidentEstimate>
    {
        public VehicleAccidentEstimateDbMapping()
        {
            HasOptional(x => x.fk_VehicleAccidentClaim).WithMany().HasForeignKey(x => x.AccidentClaimId).WillCascadeOnDelete(false);
        }
    }
}
