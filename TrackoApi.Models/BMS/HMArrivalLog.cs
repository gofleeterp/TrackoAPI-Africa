using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.BMS
{
    [Table("tHMArrivalLog")]
    public class HMArrivalLog : AuditableEntity
    {
        public long HMArrivalId { get; set; }
        [ForeignKey("HMArrivalId")]
        public virtual HMArrival fk_HMArrival { get; set; }

        public long TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }

        public long? AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Ledger fk_Account { get; set; }

        [Required]
        public long ExpenseId { get; set; }
        [ForeignKey("ExpenseId")]
        public virtual ExpenseMaster fk_Expense { get; set; }

        public long? SettlementId { get; set; }
        [ForeignKey("SettlementId")]
        public virtual VehicleTripSettlement fk_Settlement { get; set; }

        public long? CNId { get; set; }
        [ForeignKey("CNId")]
        public virtual CNMaster fk_CN { get; set; }

        public decimal Qty { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public decimal TDSRate { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal NetPayable { get; set; }
        public bool CalcTDS { get; set; } = false;
        public bool IsApproved { get; set; } = true;
        [MaxLength(150)]
        public string BatchId { get; set; }
        public long? ViewId { get; set; }
        [MaxLength(1000)]
        public string Remark { get; set; }
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(JsonData)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(JsonData)): _dt;
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
