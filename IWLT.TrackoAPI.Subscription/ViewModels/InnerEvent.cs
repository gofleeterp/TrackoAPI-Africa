using System;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public class InnerEvent
    {
        public string EventLogId { get; set; }
        public string Receiver { get; set; }
        public string Sender { get; set; }
        public DateTimeOffset EventReceivedOn { get; set; }
        public EventNotification Event { get; set; }
    }
}