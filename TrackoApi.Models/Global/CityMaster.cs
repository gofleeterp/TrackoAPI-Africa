using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Runtime.Serialization;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.Global
{
    [Table("mCityMaster")]
    public class CityMaster : AuditableEntity
    {

        [Column("CityName"), Index("IDX_mCityMaster_CityName", IsUnique = true, Order = 1), MaxLength(150)]
        public string CityName { get; set; }

        [Column("CityAbbr"), Index("IDX_mCityMaster_CityAbbr", IsUnique = true, Order = 1), MaxLength(100)]
        public string CityAbbr { get; set; }

        [Column("StateId"), ForeignKey("fk_State"), Required, Index("IDX_mCityMaster_CityAbbr", IsUnique = true, Order = 2), Index("IDX_mCityMaster_CityName", IsUnique = true, Order = 2)]

        public long StateId { get; set; }
        public virtual GenericMaster fk_State { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        [ForeignKey("fk_ControllingOffice"), Column("ControllingOfficeId")]
        public long? ControllingOfficeId { get; set; }
        public virtual OfficeMaster fk_ControllingOffice { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// If value is true user cannot modify/delete this entity
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsReserved { get; set; } = false;
        [MaxLength(100)]
        public string PostalCode { get; set; }
        public string GooglePlaceId { get; set; }
        public string GoogleData { get; set; }
        [Precision(28, 15)]
        public decimal Latitude { get; set; }
        [Precision(28, 15)]
        public decimal Longitude { get; set; }

        public List<CityMaster> Children { get; set; }
        [ForeignKey("ParentCityId")]
        public virtual CityMaster fk_Parent { get; set; }
        public long? ParentCityId { get; set; }
        [Column("DistrictId"), ForeignKey("fk_District")]
        public long? DistrictId { get; set; }
        public virtual CityMaster fk_District { get; set; }
        //[IgnoreDataMember]
        ////public DbGeography GeographyPoint { get; set; }

        //public string GeographyAsText
        //{ 
        //    get {                
        //       return GeographyPoint?.AsText(); 
        //    }
        //    set => GeographyPoint = string.IsNullOrWhiteSpace(value)?null: DbGeography.FromText(value, 4326); 
        //}

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Latitude > 0 || Longitude > 0 && GeographyPoint == null)
        //    {
        //        string errorMessage = "";
        //        try
        //        {
        //            GeographyPoint = DbGeography.FromText($"POINT({Latitude} {Longitude})", 24378);
        //        }
        //        catch (Exception ex)
        //        {
        //            errorMessage = ex.GetBaseException().Message;
        //        }
        //        if (!string.IsNullOrWhiteSpace(errorMessage))
        //        {
        //            yield return new ValidationResult(errorMessage, new[] { "Latitude", "Longitude" });
        //        }

        //    }
        //}
    }
}
