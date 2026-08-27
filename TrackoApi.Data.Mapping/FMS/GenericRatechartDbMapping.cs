using System.Data.Entity.ModelConfiguration;
using System.Runtime.InteropServices;
using TrackoApi.Models.FMS;

namespace TrackoApi.Data.Mapping
{
    public class GenericRatechartDbMapping : EntityTypeConfiguration<VehicleConfigurationLog>
    {
        public GenericRatechartDbMapping()
        {
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_SubType).WithMany().HasForeignKey(x => x.SubTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Vehicle).WithMany().HasForeignKey(x => x.VehicleId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_VehicleType).WithMany().HasForeignKey(x => x.VehicleTypeId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Route).WithMany().HasForeignKey(x => x.RouteId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Zone).WithMany().HasForeignKey(x => x.ZoneId).WillCascadeOnDelete(false);

            HasOptional(x => x.fk_State).WithMany().HasForeignKey(x => x.StateId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Party).WithMany().HasForeignKey(x => x.PartyId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_Material).WithMany().HasForeignKey(x=>x.MaterialId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_TripNature).WithMany().HasForeignKey(x => x.TripNatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GenericRef1).WithMany().HasForeignKey(x => x.GenericRef1Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_GenericRef2).WithMany().HasForeignKey(x => x.GenericRef2Id).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Script).WithMany().HasForeignKey(x => x.ScriptId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Spare).WithMany().HasForeignKey(x => x.SpareId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_AccountGroup).WithMany().HasForeignKey(x => x.AccountGroupId).WillCascadeOnDelete(false);
        }
    }
}
