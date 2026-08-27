using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Global
{
    [Table("mPostalAddress")]
    public class PostalAddress:AuditableEntity
    {
        [MaxLength(100)]
        public string UnitNo { get; set; }
        [MaxLength(200)]
        public string AddressLine1 { get; set; }
        [MaxLength(200)]
        public string AddressLine2 { get; set; }
        [MaxLength(200)]
        public string AddressLine3 { get; set; }
        [MaxLength(100)]
        public string Landmark { get; set; }
        [MaxLength(100)]
        public string EmailAddress { get; set; }
        [MaxLength(100)]
        public string AltEmailAddress { get; set; }
        [MaxLength(25)]
        public string ContactNumber { get; set; }
        [MaxLength(100)]
        public string ContactPerson { get; set; }
        [MaxLength(25)]
        public string AltContactNumber { get; set; }
        [MaxLength(100)]
        public string AltContactPerson { get; set; }
        public long? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual CityMaster fk_City { get; set; }
        public long? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual GenericMaster fk_State { get; set; }

        public long? CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country fk_Country { get; set; }
        [MaxLength(6)]
        public string PostalCode { get; set; }
        [MaxLength(300)]
        public string FullAddress { get; set; }
        [MaxLength(100)]
        public string BatchId { get; set; }
    }

    [Table("mCountry")]
    public class Country:Entity
    {
        [MaxLength(100)]
        public string CountryName { get; set; }
        [MaxLength(100)]
        public string Code { get; set; }
        [MaxLength(4)]
        public string CurrencyCode { get; set; }
        [MaxLength(20)]
        public string Currency { get; set; }
        [MaxLength(20)]
        public string CurrencySymbol { get; set; }
    }
}
