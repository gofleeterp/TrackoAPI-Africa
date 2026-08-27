using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;

namespace TrackoApi.Data
{
    public static class GenerateMigrationScript
    {
        //public class ScriptedDbContext : TrackoApiDbContext
        //{
        //    public ScriptedDbContext(string connectionstring):base(connectionstring)
        //    {

        //    }
        //    override In
        //}
        public static string GenerateSqlScript(string connectionstring)
        {
            string script = "";
            try
            {
               
                var dgc = new DbMigrationsConfiguration<TrackoApiDbContext>();
                
                dgc.AutomaticMigrationDataLossAllowed = true;
                dgc.AutomaticMigrationsEnabled = true;
                dgc.SetSqlGenerator("System.Data.SqlClient", new CustomSqlGenerator());
                dgc.ContextKey = "TrackoApi.Data.TrackoApiDbContext";
                dgc.TargetDatabase = new DbConnectionInfo(connectionstring, "System.Data.SqlClient"); 
                
                var dm = new DbMigrator(dgc,new TrackoApiDbContext(null,connectionstring));
                var dmc = new MigratorScriptingDecorator(dm);
                script = dmc.ScriptUpdate(null, null);
                
            }
            catch(Exception ex)
            {
                script=ex.GetBaseException().ToString();
            }
            if (!string.IsNullOrWhiteSpace(script))
            {
                var dir = AppDomain.CurrentDomain.BaseDirectory + $"\\ScripMigrations\\AutoMigrationScript";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(Path.Combine(dir, $"{Helper.LoggedInTenantId}-{DateTime.Now:yyyyMMdd-HH-mm}.sql"), script);
            }
            if (string.IsNullOrWhiteSpace(script)) script = "Database is upto date.";
            return script;
        }
    }
}
