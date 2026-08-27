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
    internal class CityMasterDbMapping : EntityTypeConfiguration<CityMaster>
    {
        public CityMasterDbMapping()
        {
            HasRequired(x => x.fk_State)
                .WithMany()
                .HasForeignKey(x=>x.StateId)
                .WillCascadeOnDelete(false);
            HasOptional(x => x.fk_ControllingOffice)
                .WithMany(x=>x.ControlledCities)
                .HasForeignKey(x => x.ControllingOfficeId)
                .WillCascadeOnDelete(false);
            Property(x => x.CityName).IsRequired();
            Property(x => x.CityAbbr).IsRequired();
            HasMany(x=>x.Children).WithOptional(x=>x.fk_Parent).HasForeignKey(x=>x.ParentCityId).WillCascadeOnDelete(false);
            HasOptional(x=>x.fk_District).WithMany().HasForeignKey(x=>x.DistrictId).WillCascadeOnDelete(false);
        }
    }
}
