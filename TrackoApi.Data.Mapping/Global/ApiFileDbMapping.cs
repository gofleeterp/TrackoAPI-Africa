using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping.Global
{
    public class ApiFileDbMapping:EntityTypeConfiguration<ApiFile>
    {
        public ApiFileDbMapping()
        {
            //Ignore(x => x.Stream);
            HasRequired(x=>x.fk_Related).WithMany().HasForeignKey(x=>x.RelatedId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_Nature).WithMany().HasForeignKey(x=>x.NatureId).WillCascadeOnDelete(false);
        }
    }
}
