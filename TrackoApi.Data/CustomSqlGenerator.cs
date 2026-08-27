using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using TrackoApi.Models.AMS;

namespace TrackoApi.Data
{
    internal static class DbModelBuilderExtensions
    {
        public static void RegisterAttributeAsColumnAnnotation<TAttribute>(this DbModelBuilder modelBuilder)
            where TAttribute : class
        {
            modelBuilder.Properties()
                .Having(x => x.GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault())
                .Configure((config, attribute) => config.HasColumnAnnotation(typeof(TAttribute).Name, attribute));
        }

        public static bool IsView(this string tableName) => tableName.StartsWith("View_") || tableName.StartsWith("dbo.View_");
    }
    public class CustomSqlGenerator : SqlServerMigrationSqlGenerator
    {
        private readonly List<string> _views=new List<string>();
        protected override void DropDefaultConstraint(string table, string column, IndentedTextWriter writer)
        {
            if (!_views.Contains(table)&&!table.IsView())
                base.DropDefaultConstraint(table, column, writer);
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            if (!createTableOperation.Annotations.ContainsKey("IsView") && !createTableOperation.Name.IsView())
            {
                base.Generate(createTableOperation);
            }
            else
            {
                _views.Add(createTableOperation.Name);
            }
        }
       
        protected override void Generate(AlterTableOperation alterTableOperation)
        {
            if (!alterTableOperation.Annotations.ContainsKey("IsView") && !alterTableOperation.Name.IsView())
            {
                base.Generate(alterTableOperation);
            }
            else
            {
                _views.Add(alterTableOperation.Name);
            }
        }
        protected override void Generate(DropTableOperation dropTableOperation)
        {
            if (!dropTableOperation.RemovedAnnotations.ContainsKey("IsView")&&!dropTableOperation.Name.IsView())
            {
                base.Generate(dropTableOperation);
            }
            else
            {
                _views.Add(dropTableOperation.Name);
            }
        }

        protected override void Generate(RenameTableOperation renameTableOperation)
        {
            if (renameTableOperation.Name.IsView() || renameTableOperation.NewName.IsView())
            {
               _views.Add(renameTableOperation.Name);
                _views.Add(renameTableOperation.NewName);
            }
            else
            {
                base.Generate(renameTableOperation);
            }
            
        }

        protected override void Generate(ColumnModel column, IndentedTextWriter writer)
        {
            SetColumnDataType(column);
            SetAnnotatedColumn(column);
            base.Generate(column, writer);
        }

        protected override void Generate(AddColumnOperation addColumnOperation)
        {
            if (!_views.Contains(addColumnOperation.Table) && !addColumnOperation.Table.IsView())
            {                
                base.Generate(addColumnOperation);
            }
        }

        protected override void Generate(AddPrimaryKeyOperation addPrimaryKeyOperation)
        {
            if (!_views.Contains(addPrimaryKeyOperation.Table) && !addPrimaryKeyOperation.Table.IsView())
                base.Generate(addPrimaryKeyOperation);
        }

        protected override void Generate(DropColumnOperation dropColumnOperation)
        {
            if (!_views.Contains(dropColumnOperation.Table))
                base.Generate(dropColumnOperation);
        }

        protected override void Generate(DropPrimaryKeyOperation dropPrimaryKeyOperation)
        {
            if (!_views.Contains(dropPrimaryKeyOperation.Table) && !dropPrimaryKeyOperation.Table.IsView())
                base.Generate(dropPrimaryKeyOperation);
        }

        protected override void Generate(AlterColumnOperation alterColumnOperation)
        {
            if (!_views.Contains(alterColumnOperation.Table) && !alterColumnOperation.Table.IsView())
                base.Generate(alterColumnOperation);
        }

        protected override void Generate(RenameColumnOperation renameColumnOperation)
        {
            if (!_views.Contains(renameColumnOperation.Table) && !renameColumnOperation.Table.IsView())
                base.Generate(renameColumnOperation);
        }
        
        protected override void Generate(UpdateDatabaseOperation updateDatabaseOperation)
        {
            base.Generate(updateDatabaseOperation);
        }

        protected override void Generate(SqlOperation sqlOperation)
        {
            base.Generate(sqlOperation);
        }

        private static void SetColumnDataType(ColumnModel column)
        {
            // xml type
            if (column.Annotations.ContainsKey("XmlSqlType"))
            {
                column.StoreType = "xml";
            }
        }

        private void SetAnnotatedColumn(ColumnModel col)
        {
            AnnotationValues values;
            if (col.Annotations.TryGetValue("SqlDefaultValue", out values))
            {
                col.DefaultValueSql = (string)values.NewValue;
            }
        }
    }

}
