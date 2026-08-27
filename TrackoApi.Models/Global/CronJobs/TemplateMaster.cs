using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global.CronJobs
{
    [Table("mTemplateMaster")]
    public class TemplateMaster: AuditableEntity
    {
        /// <summary>
        /// TripAdvance//HireSlip//Trip// and so on
        /// </summary>
        [Index("IX_Template_Unique",IsUnique =true,Order =1),MaxLength(100)]
        public string EntityType { get; set; }
        /// <summary>
        /// Event name e.g. OnCreate.OnDelete OnUpdate
        /// </summary>
        [Index("IX_Template_Unique", IsUnique = true, Order = 2)]
        public EventType EventType { get; set; }
        /// <summary>
        /// It could be PartyId,VehicleId,OfficeId,UserId and So on
        /// </summary>
        [Index("IX_Template_Unique", IsUnique = true, Order = 3)]
        public long? Ref1Id { get; set; }
        /// <summary>
        /// It could be PartyId,VehicleId,OfficeId,UserId and So on
        /// </summary>
        [Index("IX_Template_Unique", IsUnique = true, Order = 4)]
        public long? Ref2Id { get; set; }
        /// <summary>
        /// It could be PartyId,VehicleId,OfficeId,UserId and So on
        /// </summary>
        [Index("IX_Template_Unique", IsUnique = true, Order = 5)]
        public long? Ref3Id { get; set; }
        [Index("IX_Template_Unique", IsUnique = true, Order = 6)]
        public NotificationType MessageType { get; set; }
        public string Template { get; set; }
        public long? ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual ScheduleLog fk_Schedule { get; set; }
    }

    public enum EventType
    {
        OnCreate=0,
        OnUpdate=1,
        OnDelete=2,
        OnSuspend=3,
        OnSchedule=4,
        OnExtra1=5,
        OnExtra2=6
    }
}
