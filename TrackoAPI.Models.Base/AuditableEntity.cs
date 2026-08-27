using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.Base
{
    public interface IAuditableInfraEntity
    {
        long CreatedSessionId { get; set; }

        DateTime CreatedDOE { get; set; }

        long? ModifiedSessionId { get; set; }

        DateTime? ModifiedDOE { get; set; }
        [MaxLength(100)]
        string SecuredByTenantId { get; set; }
    }
    public interface IAuditableEntity: IEntity
    {
        
        long CreatedSessionId { get; set; }
        
        DateTime CreatedDOE { get; set; }
        
        long? ModifiedSessionId { get; set; }
        
        DateTime? ModifiedDOE { get; set; }

        long? PageId { get; set; }
        [NotMapped]
        long? AutoStationaryFieldId { get; set; }
    }
    public class AuditableEntity:Entity,IAuditableEntity
    {
        [Column("CSID"),AuditIgnore]
        public long CreatedSessionId { get; set; }
        [Column("CDOE"), AuditIgnore]
        public DateTime CreatedDOE { get; set; }
        [Column("MSID"), AuditIgnore]
        public long? ModifiedSessionId { get; set; }
        [Column("MDOE"), AuditIgnore]
        public DateTime? ModifiedDOE { get; set; }
        public long? PageId { get; set; }
        [NotMapped]
        public long? AutoStationaryFieldId { get; set; } = 0;
    }
    public class LinkedEntity : AuditableEntity, ILinkedEntity
    {
       public long? NextLogId { get; set; }
        public LinkedEntity fk_NextLog { get; set; }
        public long? PreviousLogId { get; set; }
        public LinkedEntity fk_PreviousLog { get; set; }
    }
    public interface ILinkedEntity: IAuditableEntity
    {
        long? NextLogId { get; set; }
        [ForeignKey("NextLogId")]
        LinkedEntity fk_NextLog { get; set; }
        long? PreviousLogId { get; set; }
        [ForeignKey("PreviousLogId")]
        LinkedEntity fk_PreviousLog { get; set; }
    }
    public static class AuditHelper
    {
        public static string GetDbEntryDelta(this DbEntityEntry entry,ObjectState state)
        {
            var jsonSetting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new IgnoreAuditContractResolver<AuditIgnoreAttribute>()
            };
            try
            {
                if (state == ObjectState.Modified)
                {
                    var ento = entry.OriginalValues.ToObject();
                    var entc = entry.CurrentValues.ToObject();
                    var origional = JsonConvert.SerializeObject(ento, jsonSetting);
                    var current = JsonConvert.SerializeObject(entc, jsonSetting);
                    var jdp = new JsonDiffPatch();
                    var left = JToken.Parse(origional);
                    var right = JToken.Parse(current);
                    return jdp.Diff(left, right)?.ToString(Formatting.Indented);
                }

                if (state == ObjectState.Deleted)
                {
                    var origional = "";
                    var current = JsonConvert.SerializeObject(entry.Entity, jsonSetting);
                    var jdp = new JsonDiffPatch();
                    var left = JToken.Parse(origional);
                    var right = JToken.Parse(current);
                    return jdp.Diff(left, right)?.ToString(Formatting.Indented);
                }
            }
            catch (Exception e)
            {
                var origional = "";
                var current = JsonConvert.SerializeObject(entry.Entity, jsonSetting);
                var jdp = new JsonDiffPatch();
                var left = JToken.Parse(origional);
                var right = JToken.Parse(current);
                return jdp.Diff(left, right)?.ToString(Formatting.Indented);
            }

            return "";
        }

        public static AccessType StateToAccessType(this ObjectState state)
        {
            switch (state)
            {
                case ObjectState.Unchanged:
                    return AccessType.Viewed;
                case ObjectState.Added:
                    return AccessType.Created;
                case ObjectState.Modified:
                    return AccessType.Updated;
                case ObjectState.Deleted:
                    return AccessType.Deleted;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
