using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS.Loan;

namespace TrackoApi.Data.Mapping
{
    internal class LoanVehicleLogDbMapping : EntityTypeConfiguration<LoanVehicleLog>
    {
        public LoanVehicleLogDbMapping()
        {
            HasRequired(x => x.fk_Loan).WithMany().HasForeignKey(x=>x.LoanId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
        }
    }
}
