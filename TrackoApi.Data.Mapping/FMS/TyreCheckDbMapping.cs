using System.Data.Entity.ModelConfiguration;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class TyreCheckDbMapping : EntityTypeConfiguration<TyreCheck>
    {
        public TyreCheckDbMapping()
        {
            HasRequired(x => x.fk_Vehicle).WithMany().HasForeignKey(x=>x.VehicleId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Tyre).WithMany().HasForeignKey(x => x.TyreId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_NextLog).WithMany().HasForeignKey(x => x.NextLogId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_PreviousLog).WithMany().HasForeignKey(x => x.PreviousLogId).WillCascadeOnDelete(false);

        }
    }
}
