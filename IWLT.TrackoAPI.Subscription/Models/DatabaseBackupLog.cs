using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public class DatabaseBackupLog
    {
        public DatabaseBackupLog()
        {
            Id = Guid.NewGuid().ToString("N");
        }
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        public string TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual TenantMaster Tenant { get; set; }
        public DateTime StartDate{ get; set; }
        public DateTime FinishDate { get; set; }
        [MaxLength(500)]
        public string LocalFilePath { get; set; }

        public double LocalFileSize { get; set; }
        [Column("IsPublished")]
        public bool IsPublished { get; set; }
        [MaxLength(4000)]
        public string RemoteServerPath { get; set; }
        public double RemoteFileSize { get; set; }
        public bool IsBackupFailed { get; set; }
        [MaxLength(4000)]
        public string Exception { get; set; }
    }
}
