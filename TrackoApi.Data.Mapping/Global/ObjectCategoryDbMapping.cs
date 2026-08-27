using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Mapping.Global
{
    public class ObjectCategoryDbMapping:EntityTypeConfiguration<ObjectCategory>
    {
        public ObjectCategoryDbMapping()
        {
            HasRequired(x=>x.fk_RoleType).WithMany().HasForeignKey(x=>x.RoleTypeId).WillCascadeOnDelete(false);
            HasRequired(x=>x.fk_CategoryType).WithMany().HasForeignKey(x=>x.CategoryTypeId).WillCascadeOnDelete(true);
            HasMany(x=>x.Objects).WithRequired(x=>x.fk_Category).HasForeignKey(x=>x.CategoryId).WillCascadeOnDelete(false);
            //HasMany(x=>x.ObjectClasses).WithRequired(x=>x.Category).HasForeignKey(x=>x.CategoryId).WillCascadeOnDelete(true);
        }
    }

    public class ObjectClassDbMapping : EntityTypeConfiguration<ObjectClass>
    {
        public ObjectClassDbMapping()
        {
            //HasMany(x => x.ObjectMappings).WithRequired(x=>x.fk_Class).HasForeignKey(x=>x.ClassId).WillCascadeOnDelete(true);
            HasRequired(x=>x.Category).WithMany(x=>x.ObjectClasses).HasForeignKey(x=>x.CategoryId).WillCascadeOnDelete(true);
        }
    }
    public class ObjectClassMapDbMapping : EntityTypeConfiguration<ObjectClassMap>
    {
        public ObjectClassMapDbMapping()
        {
            //HasMany(x => x.ObjectMappings).WithRequired(x=>x.fk_Class).HasForeignKey(x=>x.ClassId).WillCascadeOnDelete(true);
            HasRequired(x => x.fk_Class).WithMany(x => x.ObjectMappings).HasForeignKey(x => x.ClassId).WillCascadeOnDelete(true);
            //HasOptional(x=>x.fk_Category).WithMany(x=>x.Objects).HasForeignKey(x=>x.CategoryId).WillCascadeOnDelete(false);
            
            Ignore(x => x.ObjectName);
        }
    }
}
