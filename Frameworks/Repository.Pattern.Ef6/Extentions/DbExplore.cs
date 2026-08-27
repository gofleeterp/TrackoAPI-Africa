using DatabaseSchemaReader;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Pattern.Ef6.Extentions
{
    public static class DbExplore
    {
        const string providername = "System.Data.SqlClient";
        public static List<SchemaTableName> ReadSchema(this DbContext db)
        {
            //Create the database reader object.
            var connectionString=db.Database.Connection.ToString();
;            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            return schema.Tables.Select(t => new SchemaTableName
            {
                Name = t.Name,
                Columns = t.Columns?.Select(c => new SchemaTableColumnInfo { Name = c.Name, DataType = c.DataType.TypeName, ForeignKeyTableName = c.ForeignKeyTableName, IsForeignKey = c.IsForeignKey, IsPrimaryKey = c.IsPrimaryKey }).ToList(),
                Schema = t.SchemaOwner
            }).ToList();
        }
        public static SchemaTableName ReadSchema(this DbContext db, string tableName)
        {
            //Create the database reader object.
            var connectionString = db.Database.Connection.ToString();
            var dbReader = new DatabaseReader(connectionString, providername);
            var t = dbReader.Table(tableName);
            if (t == null) return null;
            return new SchemaTableName
            {
                Name = t.Name,
                Columns = t.Columns?.Select(c => new SchemaTableColumnInfo { Name = c.Name, DataType = c.DataType.TypeName, ForeignKeyTableName = c.ForeignKeyTableName, IsForeignKey = c.IsForeignKey, IsPrimaryKey = c.IsPrimaryKey }).ToList(),
                Schema = t.SchemaOwner
            };
        }
    }
    public class SchemaTableName
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public List<SchemaTableColumnInfo> Columns { get; set; } = new List<SchemaTableColumnInfo>();

    }
    public class SchemaTableColumnInfo
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsForeignKey { get; set; } = false;
        public bool IsPrimaryKey { get; set; } = false;
        public string ForeignKeyTableName { get; set; }
    }
}
