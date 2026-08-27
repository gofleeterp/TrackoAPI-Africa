using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoAPI.ViewModels.Integration;

namespace Tenant.Models
{
    public class JobTrack
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [MaxLength(255)]
        public string JobLogId { get; set; }
        public string EventLogId { get; set; }
        public int EventCode { get; set; }
        [ForeignKey("EventCode")]
        public virtual IntegrationEventMaster fk_Event { get; set; }
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
        //public EventNotification EventData
        //{
        //    get
        //    {
        //        return string.IsNullOrWhiteSpace(EventBody)|| HasMultipleEvent ? null : JsonConvert.DeserializeObject<EventNotification>(EventBody);
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            EventBody = JsonConvert.SerializeObject(value);
        //        }
        //    }
        //}
        //public List<EventNotification> Events
        //{
        //    get
        //    {
        //        return string.IsNullOrWhiteSpace(EventBody) || !HasMultipleEvent ? null : JsonConvert.DeserializeObject<List<EventNotification>>(EventBody);
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            EventBody = JsonConvert.SerializeObject(value);
        //        }
        //    }
        //}

        //public bool HasMultipleEvent { get; set; } = false;
    }
    public class Subscriber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        [Index(IsUnique =true),MaxLength(200)]
        public string Name { get; set; }
        public string OriginHost { get; set; }
        [Index(IsUnique = true), MaxLength(200)]
        public string Token { get; set; }
        public virtual List<IntegrationEventMaster> Events { get; set; }
    }
    public class IntegrationEventMaster
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EventCode { get; set; }
        [Index(IsUnique = true), MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public virtual List<Subscriber> Subscribers { get; set; }
        /// <summary>
        /// Allow Concurrent Processing in Hangfire if True
        /// </summary>
        public bool AllowConcurrent { get; set; } = true;
    }
}
