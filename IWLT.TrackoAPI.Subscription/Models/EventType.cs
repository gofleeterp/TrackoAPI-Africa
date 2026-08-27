using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Models
{
    public class EventType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EventCode { get; set; }
        [/*Index(IsUnique = true),*/ MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public virtual List<Subscriber> Subscribers { get; set; }
    }
}