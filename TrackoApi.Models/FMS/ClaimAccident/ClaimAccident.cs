using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Models.ClaimAccident
{
    public class ClaimAccident: AuditableEntity
    {
        [Column("OfficeId"), Required, ForeignKey("fk_Office")]
        public long? OfficeId { get; set; }
        public OfficeMaster fk_Office { get; set; }

        [Column("DocumentNo"), Required, MaxLength(20)]
        public string DocumentNo { get; set; }

        [Column("DocumentDate")]
        public DateTime? DocumentDate { get; set; }

        [Column("AccidentDate")]
        public DateTime? AccidentDate { get; set; }

        [Column("OfficeId"), Required, ForeignKey("fk_Office")]
        public long? OfficeId { get; set; }
        public OfficeMaster fk_Office { get; set; }

        [Column("AccidentPlace"), Required]
        public int AccidentPlace { get; set; }

        [Column("Driver"), Required]
        public int Driver { get; set; }




        [Column("OwnerPartyId"), ForeignKey("fk_VehicleOwner")]
        public long? OwnerPartyId { get; set; }
        public Ledger fk_VehicleOwner { get; set; }


        [Column("VehicleModelId"), Required, ForeignKey("fk_VehicleModel")]
        public long VehicleModelId { get; set; }
        public VehicleModel fk_VehicleModel { get; set; }

        [Column("RegistrationDate")]
        public DateTime? RegistrationDate { get; set; }

       

        [Column("PurchaseAmount")]
        public decimal? PurchaseAmount { get; set; }

        [Column("SoldDate")]
        public DateTime? SoldDate { get; set; }

        [Column("SoldAmount")]
        public decimal? SoldAmount { get; set; }

        [Column("ChassisNo")]
        public string ChassisNo { get; set; }

        [Column("EngineNo")]
        public string EngineNo { get; set; }

        [Column("GrossWeight")]
        public long? GrossWeight { get; set; }

        [Column("UnloadWeight")]
        public long? UnloadWeight { get; set; }

        [Column("VehicleTypeId"), ForeignKey("fk_VehicleType")]
        public long? VehicleTypeId { get; set; }

        public GenericMaster fk_VehicleType { get; set; }

        [Column("IsHireVehicle")]
        public bool IsHireVehicle { get; set; }
        [Column("IsTrailor")]
        public bool? IsTrailor { get; set; }

        [Column("AssetTypeId"), ForeignKey("fk_AssetType")]
        public long? AssetTypeId { get; set; }
    }
}
