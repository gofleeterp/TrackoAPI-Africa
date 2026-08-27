using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.Global
{
    public class NotificationLogViewModel
    {
        public string Data { get; set; }
        public long Id { get; set; }
        public int NoOfNotification { get; set; }
        public string NotificationType { get; set; }
        public bool IsSent { get; set; }
        public string Status { get; set; }
        public DateTimeOffset SentTime { get; set; }
        public string MessageId { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
    }
    public class NotificationPurchaseViewModel
    {
        public long Id { get; set; }
        public int PurchaseCount { get; set; }
        public string NotificationType { get; set; }
        public int ConsumedCount { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
        public decimal PurchaseRate { get; set; }
    }
}
