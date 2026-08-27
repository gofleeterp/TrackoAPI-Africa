using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping
{
    public class StationeryBookDbMapping : EntityTypeConfiguration<StationeryBook>
    {
        public StationeryBookDbMapping()
        {
            HasOptional(x => x.fk_Client).WithMany().HasForeignKey(x => x.ClientId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Nature).WithMany().HasForeignKey(x => x.NatureId).WillCascadeOnDelete(false);
            HasOptional(x => x.fk_Office).WithMany().HasForeignKey(x => x.OfficeId).WillCascadeOnDelete(false);
            HasRequired(x => x.fk_Type).WithMany().HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
            Property(x => x.Name).IsRequired().HasMaxLength(100);
        }
    }
    
}
