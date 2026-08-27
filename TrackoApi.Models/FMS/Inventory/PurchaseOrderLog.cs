using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS.Inventory
{
    [Table("tPurchaseOrderLog")]
    public class PurchaseOrderLog : AuditableEntity
    {
        
        public long? PRLId { get; set; }
        [ForeignKey("PRLId")]
        public virtual PurchaseRequisitionLog fk_PRL { get; set; }

        public long PurchaseOrderId { get; set; }
        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder fk_PurchaseOrder { get; set; }

        public long? SpareId { get; set; }
        [ForeignKey("SpareId")]
        public virtual SpareMaster fk_Spare { get; set; }

        public long? SpareMakeId { get; set; }
        // [ForeignKey("SpareMakeId")]
        // public virtual GenericMaster fk_SpareMake { get; set; }

        public long? UnitId { get; set; }
        [ForeignKey("UnitId")]
        public virtual UnitMaster fk_Unit { get; set; }

        public long? TyreBrandId { get; set; }
        [ForeignKey("TyreBrandId")]
        public virtual BrandMaster fk_TyreBrand { get; set; }

        public long? BatteryBrandId { get; set; }
        [ForeignKey("BatteryBrandId")]
        public virtual BatteryBrand fk_BatteryBrand { get; set; }
        

        public decimal POQty { get; set; } = 0;
        public decimal POHours { get; set; } = 0;
        public decimal PORate { get; set; } = 0;
        public decimal BasicAmount { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal SubTotal { get; set; } = 0;
        public decimal NetRate { get; set; } = 0;
        public decimal VATPercent { get; set; } = 0;
        public decimal VATAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;

        public DateTime? DeliveryDate { get; set; }

        public long? DeliveryPlaceId { get; set; }
        [ForeignKey("DeliveryPlaceId")]
        public virtual CityMaster fk_DeliveryPlace { get; set; }
        public string Remark { get; set; }

        [Column("Ref1Id"), ForeignKey("fk_Ref1")]
        public long? Ref1Id { get; set; }
        public virtual GenericMaster fk_Ref1 { get; set; }

        [Column("Ref2Id"), ForeignKey("fk_Ref2")]
        public long? Ref2Id { get; set; }
        public virtual GenericMaster fk_Ref2 { get; set; }

        [Column("Ref3Id"), ForeignKey("fk_Ref3")]
        public long? Ref3Id { get; set; }
        public virtual GenericMaster fk_Ref3 { get; set; }

        [Column("Ref4Id"), ForeignKey("fk_Ref4")]
        public long? Ref4Id { get; set; }
        public virtual GenericMaster fk_Ref4 { get; set; }
        public string Data { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> DataView
        {
            get
            {
                try
                {
                    if (Data == "{}") Data = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((Data ?? (Data = "[]"))));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                Data = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }
        }

        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((Data ?? "{}") == "{}") Data = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((Data ?? (Data = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                Data = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                Data = "[]";
            }
        }
    }
}
