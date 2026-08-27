using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tPRLog")]
    public class PurchaseRequisitionLog : AuditableEntity
    {
        
        [Column("PRId"), ForeignKey("fk_PR")]
        public long PRId { get; set; }
        public virtual PurchaseRequisition fk_PR { get; set; }
        [Column("TypeId"), Required, ForeignKey("fk_Type")]
        public long TypeId { get; set; }
        public virtual ConstantValue fk_Type { get; set; }

        [Column("SpareId"), ForeignKey("fk_Spare")]
        public long? SpareId { get; set; }
        public virtual SpareMaster fk_Spare { get; set; }
        
        [Required]
        public decimal RequestQty { get; set; } = 0;
        public decimal ApprovedQty { get; set; } = 0;
        public decimal StockQty { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        
        [Column("UnitId"), ForeignKey("fk_Unit")]
        public long? UnitId { get; set; }
        public virtual UnitMaster fk_Unit { get; set; }
        

        [Column("BatteryBrandId"), ForeignKey("fk_BatteryBrand")]
        public long? BatteryBrandId { get; set; }
        public virtual BatteryBrand fk_BatteryBrand { get; set; }

        [Column("TyreBrandId"), ForeignKey("fk_TyreBrand")]
        public long? TyreBrandId { get; set; }
        public virtual BrandMaster fk_TyreBrand { get; set; }

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

        /// <summary>
        /// 1156-Pending,1157-Approved,1158-Rejected
        /// </summary>
        public long StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual ConstantValue fk_Status { get; set; }

        public string Ref1 { get; set; }
        public string Ref2 { get; set; }

        public DateTime? APRLDateTime { get; set; }
        public string APRLRemark { get; set; }
        public long? APRLCSID { get; set; }
        public long? APRLUserId { get; set; }
        public bool IsAPRLRequired { get; set; } = false;

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
