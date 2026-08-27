using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.Integration
{
    public class EventNotification
    {
        [Required]
        public int EventCode { get; set; }
        [Required]
        public DateTimeOffset EventTime { get; set; }
        public int Retry { get; set; } = 0;
        public IDictionary<string,object> Properties { get; set; }
        public string EncryptedMessage { get; set; }
    }
    public class InnerEvent
    {
        public InnerEvent()
        {
            Events=new List<EventNotification>();
        }
        public string EventLogId { get; set; }
        public string Receiver { get; set; }
        public string Sender { get; set; }
        public DateTimeOffset EventReceivedOn { get; set; }
        public EventNotification Event { get; set; }
        public List<EventNotification> Events { get; set; }
        public bool HasMultipleEvent { get; set; } = false;
    }
}
