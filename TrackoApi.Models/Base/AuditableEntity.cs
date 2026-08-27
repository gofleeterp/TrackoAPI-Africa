using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Base
{
    public interface IAuditableEntity: IEntity
    {
        
        long CreatedSessionId { get; set; }
        
        DateTime CreatedDOE { get; set; }
        
        long? ModifiedSessionId { get; set; }
        
        DateTime? ModifiedDOE { get; set; }

        long? PageId { get; set; }
    }
    public class AuditableEntity:Entity,IAuditableEntity
    {
        [Column("CSID")]
        public long CreatedSessionId { get; set; }
        [Column("CDOE")]
        public DateTime CreatedDOE { get; set; }
        [Column("MSID")]
        public long? ModifiedSessionId { get; set; }
        [Column("MDOE")]
        public DateTime? ModifiedDOE { get; set; }
        public long? PageId { get; set; }
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
}
