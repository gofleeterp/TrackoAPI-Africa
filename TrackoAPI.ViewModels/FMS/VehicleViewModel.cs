using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.FMS
{
    public class VehicleViewModel
    {
        [Required, MaxLength(20)]
        public string VehicleNo { get; set; }

        [Required, MaxLength(20)]
        public string VehicleRegNo { get; set; }

        [Required, MaxLength(4)]
        public string YearOfManufacter { get; set; }

        [Required, ForeignKey("OfficeName")]
        public int OfficeId { get; set; }

        [Column("OwnerPartyId"), Required, ForeignKey("fk_VehicleOwner")]
        public int OwnerPartyId { get; set; }

        [ Required]
        public int VehicleModelId { get; set; }

        public DateTime? RegistrationDate { get; set; }
        public DateTime? PurchaseDate { get; set; }

        public decimal? PurchaseAmount { get; set; }
        public DateTime? SoldDate { get; set; }
        public decimal? SoldAmount { get; set; }
        public string ChassisNo { get; set; }
        public string EngineNo { get; set; }
        public int? GrossWeight { get; set; }
        public int? UnloadWeight { get; set; }
        public int? VehicleTypeId { get; set; }

        [Required]
        public int StatusId { get; set; }
        public virtual List<TyresOnVehicle> Tyres { get; set; }
    }

    public class TyresOnVehicle
    {
        public string TyreNo { get; set; }
        public long TyreId { get; set; }
        public DateTime OnDate { get; set; }
    }
    
}
