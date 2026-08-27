using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class SpareFitmentPositionDbMapping : EntityTypeConfiguration<SpareFitmentPosition>
    {
        public SpareFitmentPositionDbMapping()
        {
            //HasOptional(x => x.fk_FitmentPostion).WithOptionalPrincipal().WillCascadeOnDelete(false);
            //HasOptional(x => x.fk_Spare).WithOptionalPrincipal().WillCascadeOnDelete(false);
            //HasRequired(x => x.fk_Spare).WithMany().HasForeignKey(x => x.SpareId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_FitmentPostion).WithMany().HasForeignKey(x => x.FitmentPositionId).WillCascadeOnDelete(false);
        }
    }
}
