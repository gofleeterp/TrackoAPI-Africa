using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class VehicleAccidentClaimDbMapping : EntityTypeConfiguration<VehicleAccidentClaim>
    {
        public VehicleAccidentClaimDbMapping()
        {
            HasRequired(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_AccidentPlace).WithMany().HasForeignKey(x => x.AccidentPlaceId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Driver).WithMany().HasForeignKey(x => x.DriverId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_InsuranceCompany).WithMany().HasForeignKey(x => x.InsCompanyId).WillCascadeOnDelete(false);

        }
    }
}
