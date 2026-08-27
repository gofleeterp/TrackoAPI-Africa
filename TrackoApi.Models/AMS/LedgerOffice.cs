using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.AMS
{
    [Table("mLedgerOffice")]
    public class LedgerOffice : AuditableEntity
    {
        [ForeignKey("fk_Ledger"), Required,Index("IDX_LedgerOffice_UniqueKey",IsUnique = false,Order = 0)]
        public long LedgerId { get; set; }
        public virtual Ledger fk_Ledger { get; set; }

        //[ForeignKey("fk_City"), Required, Index("IDX_LedgerOffice_UniqueKey", IsUnique = true, Order = 1)]
        //public long? CityId { get; set; }
        //public virtual CityMaster fk_City { get; set; }
        
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public string GSTIN { get; set; }
        [MaxLength(200),Index("IDX_LedgerOffice_UniqueKey", IsUnique = false, Order = 1)]
        public string PlantName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string BillingAddress { get; set; }
        public bool IsDefault { get; set; }
    }
}