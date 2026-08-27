using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public class JobTrack
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [MaxLength(255)]
        public string JobLogId { get; set; }
        public string EventLogId { get; set; }
        public int? EventCode { get; set; }
        [ForeignKey("EventCode")]
        public virtual EventType fk_EventType { get; set; }
        public string SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual Subscriber fk_Sender { get; set; }
        public string TenantId { get; set; }
        [ForeignKey("TenantId")]
        public virtual TenantMaster fk_Tenant { get; set; }
        public string EventBody { get; set; }
        public bool IsProcessed { get; set; }
        public string Error { get; set; }
        public DateTimeOffset? ProcessedTime { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }=DateTimeOffset.Now;
        public EventNotification EventData
        {
            get
            {
                return string.IsNullOrWhiteSpace(EventBody) ? null : JsonConvert.DeserializeObject<EventNotification>(EventBody);
            }
            set
            {
                if (value != null)
                {
                    EventBody = JsonConvert.SerializeObject(value);
                }
            }
        }
    }
}
