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
    public class ModuleDbMapping : EntityTypeConfiguration<ApiViewModule>
    {
        public ModuleDbMapping()
        {
            //HasMany(x=>x.SubModules).WithRequired(x=>x.ParentModule).HasForeignKey(x=>x.ParentModuleId).WillCascadeOnDelete(false);
            HasOptional(x=>x.ParentApiViewModule).WithMany(x=>x.SubModules).HasForeignKey(x=>x.ParentModuleId).WillCascadeOnDelete(false);
            HasMany(x=>x.Views).WithRequired(x=>x.ApiViewModule).HasForeignKey(x=>x.ModuleId).WillCascadeOnDelete(false);
        }
    }
}
