using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

namespace TrackoApi.Models.BMS
{
    public class SalesOrderRequest : AuditableEntity, IValidatableObject
    {
        [MaxLength(30), StationaryCheck]
        public string RequestNo { get; set; }
        public DateTime OrderDate { get; set; }
        public long? PartyId { get; set; }
        [ForeignKey("PartyId")]
        public virtual Ledger fk_Party { get; set; }
        public long? PickupLocationId { get; set; }
        [ForeignKey("PickupLocationId")]
        public virtual CityMaster fk_PickupLocation { get; set; }
        public long? DropLocationId { get; set; }
        [ForeignKey("DropLocationId")]
        public virtual CityMaster fk_DropLocation { get; set; }

        public long? ZoneId { get; set; }
        [ForeignKey("ZoneId")]
        public virtual GenericMaster fk_Zone { get; set; }
        public long? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual GenericMaster fk_State { get; set; }
        public decimal LoadValue1 { get; set; }
        public decimal LoadValue2 { get; set; }

        public long? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual UnitMaster fk_Unit { get; set; }        

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if((LoadValue1+LoadValue2)<=0)
            {
                yield return new ValidationResult("Load Value should be greater than Zero");
            }
        }
    }
}
