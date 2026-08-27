using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Core.Helpers;

namespace TrackoApi.Models.Base
{
    public interface IEntity
    {
        [Key, Column("Id",Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        long Id { get; set; }
        [NotMapped]
        ObjectState ObjectState { get; set; }
        //List<string> ModifiedProperties { get; set; }
        [MaxLength(100)]
        string SecuredByTenantId { get; set; }
    }
    public class Entity : IEntity
    {
       // [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual long Id { get; set; }
        [NotMapped, AuditIgnore]
        public ObjectState ObjectState { get; set; }
        //public List<string> ModifiedProperties { get; set; }
        [MaxLength(100)]
        public string SecuredByTenantId { get; set; }

    }
    public interface IAprovalEntity
    {
        DateTime? APRLDateTime { get; set; }
        string APRLRemark { get; set; }
        long? APRLSID { get; set; }
        long? APRLUserId { get; set; }
        bool IsAutoAPRL { get; set; }
    }
}
