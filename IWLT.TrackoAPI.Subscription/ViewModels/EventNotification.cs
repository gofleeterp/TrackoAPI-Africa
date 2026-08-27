using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public class EventNotification
    {
        [Required]
        public int EventCode { get; set; }
        [Required]
        public DateTimeOffset EventTime { get; set; }
        public int Retry { get; set; } = 0;
        public IDictionary<string, object> Properties { get; set; }
    }
}