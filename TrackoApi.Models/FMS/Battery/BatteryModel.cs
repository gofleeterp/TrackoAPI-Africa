using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.FMS
{
    [Table("mBatteryBrand")]
    public class BatteryBrand : AuditableEntity
    {
        [Column("BrandName"), Required, MaxLength(100), Index("IDX_mBatteryBrand_BrandName", IsUnique = true)]
        public string BrandName { get; set; }

        public long ManufacturerId { get; set; }
        [ForeignKey("ManufacturerId")]
        public virtual GenericMaster fk_Manufacturer { get; set; }

        [Column("WarrantyPeriod")]
        public long? DefaultWarrantyPeriod { get; set; } = 0;
        public long? BudgetedAge { get; set; } = 0;

        [Column("Remark"),MaxLength(255)]
        public string Remark { get; set; }

        [Column("StatusId")]
        public virtual MasterStatus Status { get; set; }
        public long? ViewId { get; set; }

    }
}