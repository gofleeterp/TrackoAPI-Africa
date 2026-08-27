using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global.CronJobs
{
    [Table("mMessageAddress")]
    public class MessageAddress:AuditableEntity
    {
        [Index("IDX_MessageAddress_Unique",IsUnique =true,Order =1)]
        public long ContactId { get; set; }
        [ForeignKey("ContactId")]
        public virtual Contact fk_Contact { get; set; }
        [Index("IDX_MessageAddress_Unique", IsUnique = true, Order =2)]
        public long JobId { get; set; }
        [ForeignKey("JobId")]
        public virtual JobLog fk_Job { get; set; }

        public AddressType AddressType { get; set; }
    }
    
}
