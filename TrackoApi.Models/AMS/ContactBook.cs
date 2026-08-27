using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.AMS
{
    [Table("mContactBook")]
    public class Contact:AuditableEntity
    {
        [Column("FirstName"),MaxLength(100)]
        public string FirstName { get; set; }

        [Column("MiddleName"), MaxLength(100)]
        public string MiddleName { get; set; }

        [Column("LastName"), MaxLength(100)]
        public string LastName { get; set; }

        public long ContactTypeId { get; set; }
        [ForeignKey("ContactTypeId")]
        public virtual ConstantValue fk_ContactType { get; set; }

        [Column("ContactValue"), MaxLength(400),Index("IDX_Unique_Contact",IsUnique =true)]
        public string ContactValue { get; set; }
        public long? ViewId { get; set; }
        public long ContactNatureId { get; set; }
        [ForeignKey("ContactNatureId")]
        public virtual ConstantValue fk_ContactNature { get; set; }
        public long? LedgerId { get; set; }
        [ForeignKey("LedgerId")]
        public virtual Ledger fk_Ledger { get; set; }

    }
}