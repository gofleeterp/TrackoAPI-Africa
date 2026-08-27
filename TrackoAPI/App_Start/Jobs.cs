using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoAPI.Hubs;

namespace TrackoAPI
{
    public class Jobs
    {
        public static async Task BackupDataBaseEveryDay()
        {
            try
            {
                using (var db = new TenantDbContext())
                {
                    foreach (var tenant in db.Tenants.Where(x => x.IsActive).Select(x => new { x.ConnectionString, x.Id, x.ClientKey }).ToList())
                    {
                        ClientHub.BroadCastMessageToTenant(tenant.ClientKey, "Server Backup is in process.Work Performance could be poor for a while.\n Please be patient", "Server Backup Notification!!");
                        var conn = tenant.ConnectionString;
                        var backup = new DatabaseBackupLog
                        {
                            StartDate = DateTime.Now,
                            IsPublished = false,
                            TenantId = tenant.Id,
                        };
                        db.BackupLogs.Add(backup);
                        var dbname = conn.Split(';').FirstOrDefault(x => x.ToLower().Contains("database"));
                        if (!string.IsNullOrWhiteSpace(dbname))
                        {
                            var query = string.Empty;
                            try
                            {
                                var path = ConfigurationManager.AppSettings["dbbackuppath"];
                                if (string.IsNullOrWhiteSpace(path))
                                {
                                    path = "C:\\ClientDbBackup";
                                }
                                else
                                {
                                    if (!Directory.Exists(path))
                                    {
                                        Directory.CreateDirectory(path);
                                    }
                                }
                                if (!path.EndsWith("\\")) path = path + "\\";
                                var dt = DateTime.Now;
                                path = $"{path}{dt:dd-MM-yyyy}_{dbname.Split('=')[1]}_{dt:h-mm-ss tt}.bak";
                                query =
                                    $"EXEC [dbo].[sp_Backup_Database] '{dbname.Split('=')[1]}','{path}'";
                                backup.LocalFilePath = path;

                                await db.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, query);
                                backup.FinishDate = DateTime.Now;
                                //ExceptionlessClient.Default.CreateEvent()
                                //    .AddObject(new {DirectoryPath = path, DataBaseName = dbname.Split('=')[1]})
                                //    .SetSource("Database Backup").Submit();
                                backup.LocalFileSize = new FileInfo(path).Length;
                            }
                            catch (Exception ex)
                            {
                                var exc = ex.GetBaseException();
                                backup.IsBackupFailed = true;
                                backup.Exception = exc.Message + "\n" + exc.StackTrace;

                                //Ignore
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                }
                //await UploadToOneDrive();
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public static FileInfo CreateZip(string filepath)
        {
            var bakfile = new FileInfo(filepath);
            var fileName = new FileInfo(filepath.Replace(".bak", ".zip"));
            if (bakfile.Exists && !fileName.Exists)
            {
                if (fileName.Directory != null && !fileName.Directory.Exists)
                {
                    fileName.Directory.Create();
                }
                if (fileName.Exists) fileName.Delete();
                using (var zipfile = new ZipFile(fileName.FullName))
                {
                    zipfile.CompressionLevel = CompressionLevel.BestCompression;
                    zipfile.CompressionMethod = CompressionMethod.BZip2;
                    zipfile.FlattenFoldersOnExtract = true;
                    zipfile.AddFile(bakfile.FullName, "");
                    zipfile.Save();
                }
                if (fileName.Exists)
                {
                    bakfile.Delete();
                }
            }
            return fileName;
        }

        public static async Task UploadToOneDrive()
        {
            try
            {
                //var odc = OneDriverHelper.Instance;
                using (var db = new TenantDbContext())
                {
                    var files =
                        await
                            db.BackupLogs.Where(x => !x.IsBackupFailed && !x.IsPublished).Include(x => x.Tenant)
                                .GroupBy(x => new { x.TenantId, x.FinishDate.Date })
                                .ToListAsync();
                    foreach (DatabaseBackupLog log in files.Select(file => file.Where(
                        x =>
                            (x.FinishDate.Hour >= 12 && x.FinishDate.Hour <= 2) ||
                            (x.FinishDate.Hour >= 09 && x.FinishDate.Hour <= 12))).SelectMany(filestoupload =>
                            {
                                var logs = filestoupload as IList<DatabaseBackupLog> ?? filestoupload.ToList();
                                return logs;
                            }))
                    {
                        try
                        {
                            var zipFile = CreateZip(log.LocalFilePath);
                            if (zipFile.Exists)
                            {
                                //Item tenantFolder=null;
                                //if (!string.IsNullOrWhiteSpace(log.Tenant.RemoteBackupPath))
                                //{
                                //    tenantFolder=await odc.LoadFolderFromId(log.Tenant.RemoteBackupPath);
                                //}
                                //if (tenantFolder == null)
                                //{
                                //    tenantFolder = await odc.CreateFolderByTargetFolderPath("", log.Tenant.Name);
                                //}
                                //if(tenantFolder==null)continue;
                                //log.Tenant.RemoteBackupPath = tenantFolder.Id;
                                //var odcupload = await odc.UploadFile(zipFile.FullName, tenantFolder);
                                //if (odcupload != null)
                                //{
                                //    log.LocalFilePath = zipFile.FullName;
                                //    log.RemoteFileSize = zipFile.Length;
                                //    log.IsPublished = true;
                                //    log.RemoteServerPath = JsonConvert.SerializeObject(odcupload);

                                //}
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    //await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}