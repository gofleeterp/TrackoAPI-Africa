using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public class Subscriber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        [/*Index(IsUnique =true),*/MaxLength(200)]
        public string Name { get; set; }
        public string OriginHost { get; set; }
        [/*Index(IsUnique = true),*/ MaxLength(200)]
        public string Token { get; set; }
        public virtual List<EventType> SubscribedEvents { get; set; }
    }
}