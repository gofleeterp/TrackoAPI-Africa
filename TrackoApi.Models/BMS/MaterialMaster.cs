using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.BMS
{
    [Table("mMaterialMaster")]
    public class MaterialMaster:AuditableEntity
    {
        [Column("Name"),MaxLength(200)]
        public string MaterialName { get; set; }
        [Column("Code"), MaxLength(200)]
        public string Abbreviation { get; set; }

        public long MaterialGroupId { get; set; }
        [ForeignKey("MaterialGroupId")]
        public virtual MaterialGroup fk_MaterialGroup { get; set; }

        [Column("StatusId")]
        public MasterStatus Status { get; set; }
        public bool IsReserved { get; set; } = false;
        public decimal Length { get; set; } = 0;
        public decimal Breadth { get; set; } = 0;
        public decimal Height { get; set; } = 0;
        public decimal Weight { get; set; } = 0;
        public long? PartyId { get; set; }
        [ForeignKey("PartyId")]
        public virtual Ledger fk_Party { get; set; }
        public long? ViewId { get; set; }

        public decimal QtyPerPkg { get; set; } = 0;
        public long? PkgUnitId { get; set; }
        [ForeignKey("PkgUnitId")]
        public virtual  UnitMaster fk_PkgUnit { get; set; }

        public decimal PerDayConsumption { get; set; }

        public long? DeliveryLocationId { get; set; }
        [ForeignKey("DeliveryLocationId")]
        public virtual GenericMaster fk_DeliveryLocation { get; set; }

        public long? WarehouseLocationId { get; set; }
        [ForeignKey("WarehouseLocationId")]
        public virtual GenericMaster fk_WarehouseLocation { get; set; }

        public long? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual UnitMaster fk_Unit { get; set; }

        public decimal DefaultRate { get; set; } = 0;
        public virtual List<MaterialLocationMap> LocationMappings { get; set; }

        /*ZRA Field*/
        [Column("itemCd"), MaxLength(100)]
        public string itemCd { get; set; }

        [Column("orgnNatCd"), MaxLength(10)]
        public string orgnNatCd { get; set; }

        [Column("pkgUnitCd"), MaxLength(10)]
        public string pkgUnitCd { get; set; }

        [Column("qtyUnitCd"), MaxLength(10)]
        public string qtyUnitCd { get; set; }

        [Column("vatCatCd"), MaxLength(10)]
        public string vatCatCd { get; set; }
    }
    public class MaterialLocationMap : AuditableEntity
    {
        /// <summary>
        /// ConstantId:
        /// </summary>
        [Index("IX_MaterialLocationMap_Unique",IsUnique =true,Order =1)]
        public long PlantId { get; set; }
        [ForeignKey(nameof(PlantId))]
        public virtual GenericMaster fk_Plant { get; set; }
        [Index("IX_MaterialLocationMap_Unique", IsUnique = true, Order = 2)]
        public long MaterialId { get; set; }
        [ForeignKey(nameof(MaterialId))]
        public virtual MaterialMaster fk_Material { get; set; }
        /// <summary>
        /// ConstantId:1550
        /// </summary>
        [Index("IX_MaterialLocationMap_Unique", IsUnique = true, Order =3)]
        public long? LocationId { get; set; }
        [ForeignKey(nameof(LocationId))]
        public virtual GenericMaster fk_Location { get; set; }
    }
    //[Table("mMaterialParty")]
    //public class MaterialParty : AuditableEntity
    //{
    //    public long? PartyId { get; set; }
    //    [ForeignKey("PartyId")]
    //    public virtual Ledger fk_Party { get; set; }
    //    public long? MaterialId { get; set; }
    //    [ForeignKey("MaterialId")]
    //    public virtual MaterialMaster fk_Material { get; set; }

    //}
}