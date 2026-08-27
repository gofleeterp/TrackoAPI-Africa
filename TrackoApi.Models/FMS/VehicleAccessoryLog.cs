using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tVehicleAccessoryLog")]
    public class VehicleAccessoryLog : AuditableEntity
    {
        public VehicleAccessoryLog()
        {
            Data = new List<JsonDataEntity>();
        }
        public DateTime LogDate { get; set; }

        [Column("AssetId")]
        public long AssetId { get; set; }
        [ForeignKey("AssetId")]
        public virtual VehicleMaster fk_Asset { get; set; }

        public long? DriverId { get; set; }
        [ForeignKey("DriverId")]
        public virtual DriverMaster fk_Driver { get; set; }

        public long? StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual ConstantValue fk_Status { get; set; }
        [Column("SparePartId")]
        public long SparePartId { get; set; }
        [ForeignKey("SparePartId")]
        public virtual SpareMaster fk_SparePart { get; set; }
        public decimal Qty { get; set; } = 0;
        public decimal DepositedQty { get; set; } = 0;
        public decimal ScrapQty { get; set; } = 0;
        public decimal BalanceQty { get; set; } = 0;
        public long? SpareLogId { get; set; }
        [ForeignKey("SpareLogId")]
        public virtual SpareLog fk_SpareLog { get; set; }
        [MaxLength(200)]
        public string Remark { get; set; }
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {            
            get
            {
                try
                {
                    if (JsonData == "{}") JsonData = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData ?? (JsonData = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                JsonData = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }


        }
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((JsonData ?? "{}") == "{}") JsonData = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((JsonData ?? (JsonData = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                JsonData = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                JsonData = "[]";
            }
        }

    }
}