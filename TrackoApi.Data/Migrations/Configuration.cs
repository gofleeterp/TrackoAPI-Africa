using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<TrackoApiDbContext>
    {
        public Configuration()
        {
            SetSqlGenerator("System.Data.SqlClient", new CustomSqlGenerator());
            //var allowMigration = ConfigurationManager.AppSettings["AllowMigration"];
            //var enableMigration = false;
            //if (allowMigration != null && bool.TryParse(allowMigration, out enableMigration) && enableMigration)
            //{
            AutomaticMigrationsEnabled = true;
            //}
            var allowDataloss = ConfigurationManager.AppSettings["AllowDataLossMigration"];
            if (allowDataloss != null && bool.TryParse(allowDataloss, out var enableDataLossMigration) &&
                enableDataLossMigration)
            {
                AutomaticMigrationDataLossAllowed = true;
            }
            ContextKey = "TrackoApi.Data.TrackoApiDbContext";
        }

        protected override void Seed(TrackoApiDbContext context)
        {
            try
            {
                bool isnew = false;
                if (!context.ConstantTypes.Any())
                {
                    isnew = true;
                    IntializeDatabase(context);
                }
                var versionName = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.ManifestModule.Name == "TrackoAPI.dll")?.GetName()?.Version?.ToString();
                if (string.IsNullOrWhiteSpace(versionName)) return;
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"\\ScripMigrations\\{(isnew ? "" : versionName)}");
                if (!Directory.Exists(dir)) return;
                var migrationlog = context.ScriptMigrations.Where(x => x.VersionId == versionName).ToList();

                var sqlFiles = Directory.GetFiles(dir, "*.sql", SearchOption.AllDirectories).Where(x => !migrationlog.Any(y => y.ScriptFolderName == x.Replace((isnew ? Path.Combine(dir, versionName) : dir), ""))).OrderBy(x => x);
                foreach (var sql in sqlFiles)
                {
                    var commands = ReadAllCommands(sql);
                    var mig = new ApiScriptMigration
                    {
                        ObjectState = ObjectState.Added,
                        LastExecution = DateTime.Now,
                        Execute = false,
                        ReleaseDate = DateTime.Now,
                        ScriptFolderName = sql.Replace(dir, ""),
                        VersionId = versionName
                    };
                    foreach (var command in commands)
                    {
                        try
                        {
                            context.Database.ExecuteSqlCommand(command);
                        }
                        catch (SqlException ex)
                        {
                            mig.FailedScriptLog += $"\nGO\n{command}";
                        }
                    }
                    context.ScriptMigrations.AddOrUpdate(mig);
                }
                var count = context.SaveChanges();
                if (count > 0)
                {
                    ConfigurationManager.AppSettings["AllowMigration"] = "false";
                    ConfigurationManager.AppSettings["AllowDataLossMigration"] = "false";
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Gets the substring until the first dot.
        /// </summary>
        private static string GetNameUntilFirstDot(string name)
        {
            var dotIdx = name.IndexOf('.');
            if (dotIdx == 0)
            {
                throw new Exception("No '.' found in file name.");
            }
            return name.Substring(0, dotIdx);
        }

        private static void IntializeDatabase(TrackoApiDbContext context)
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory + $"\\ScripMigrations\\InitialScripts";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            else
            {
                var sequential_sqlFiles = Directory.GetFiles(dir, "*.sql").Where(x => int.TryParse(GetNameUntilFirstDot(x), out _)).OrderBy(x => int.Parse(GetNameUntilFirstDot(x)));
                foreach (var sql in sequential_sqlFiles)
                {
                    var commands = ReadAllCommands(sql);
                    var mig = new ApiScriptMigration
                    {
                        ObjectState = ObjectState.Added,
                        LastExecution = DateTime.Now,
                        Execute = false,
                        ReleaseDate = DateTime.Now,
                        ScriptFolderName = sql.Replace(dir, ""),
                        VersionId = string.Empty
                    };
                    foreach (var command in commands)
                    {
                        try
                        {
                            context.Database.ExecuteSqlCommand(command);
                        }
                        catch (SqlException ex)
                        {
                            mig.FailedScriptLog += $"\nGO\n{command}";
                        }
                    }
                    context.ScriptMigrations.AddOrUpdate(mig);
                }
                var count = context.SaveChanges();
                var nonsequential_sqlFiles = Directory.GetFiles(dir, "*.sql").Where(x => !int.TryParse(GetNameUntilFirstDot(x), out _)).OrderBy(x => x);
            }
        }
        private static IEnumerable<string> ReadAllCommands(string path)
        {
            StringBuilder sb = null;
            foreach (string line in File.ReadLines(path))
            {
                if (string.Equals(line, "GO", StringComparison.OrdinalIgnoreCase))
                {
                    if (null != sb && 0 != sb.Length)
                    {
                        string item = sb.ToString();
                        if (!string.IsNullOrWhiteSpace(item)) yield return item;
                        sb = null;
                    }
                }
                else
                {
                    if (null == sb) sb = new StringBuilder();
                    sb.AppendLine(line);
                }
            }
            if (null != sb && 0 != sb.Length)
            {
                string item = sb.ToString();
                if (!string.IsNullOrWhiteSpace(item)) yield return item;
            }
        }
    }
}