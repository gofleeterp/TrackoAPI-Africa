using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoAPI.Models.Shared;

namespace Tenant.Models
{
    public class NotificationLog
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string TenantId { get; set; }
        public string MessageId { get; set; } = "";
        public int NoOfNotification { get; set; }
        public NotificationType NotificationType { get; set; }
        public DateTimeOffset SentTime { get; set; } = DateTimeOffset.Now;
        /// <summary>
        /// Zero incase it was automatically shooted
        /// </summary>
        public long UserId { get; set; } = 0;
        public string Data { get; set; }
        public string Status { get; set; }
        public long PurchaseId { get; set; }
        [ForeignKey(nameof(PurchaseId))]
        public virtual NotificationPurchase fk_Purchase { get; set; }
        public bool IsSent { get; set; }
    }
    public class NotificationPurchase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string TenantId { get; set; }
        public int NoOfNotification { get; set; }
        public NotificationType NotificationType { get; set; }
        public DateTimeOffset PurchaseTime { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? ExpiryTime { get; set; }
        public decimal PurchaseRate { get; set; }
        public int Balance { get; set; } = 0;
        public PurchaseType PaymentStatus { get; set; }
        public virtual List<NotificationLog> Notifications { get; set; }
    }
    
}
