using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.BMS
{
    [Table("mCNExtraInfo")]
    public class CNExtraInfo : AuditableEntity
    {
        [Column("Id"), Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
        [MaxLength(100), StationaryCheck]
        public string ReferenceNo { get; set; }
        public long? CNId { get; set; }
        [ForeignKey("CNId")]
        public virtual CNMaster fk_CNMaster { get; set; }

        public long? TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }

        public DateTime? ReachDate { get; set; }
        public DateTime? UnloadDate { get; set; }

        [Required]
        public DateTime PODDate { get; set; }

        public bool IsPodOk { get; set; } = true;

        [Column("PodStatusId")]
        public long? PodStatusId { get; set; }
        [ForeignKey("PodStatusId")]
        public virtual GenericMaster fk_PodStatus { get; set; }

        public decimal ScratchQty { get; set; } = 0;
        public decimal ScratchAmount { get; set; } = 0;

        public decimal DamageQty { get; set; } = 0;
        public decimal DamageAmount { get; set; } = 0;

        public decimal ShortageQty { get; set; } = 0;
        public decimal ShortageAmount { get; set; } = 0;
        #region Dealer Settlement(DS) Details       
        public long? DSVoucherId { get; set; }
        [ForeignKey("DSVoucherId")]
        public virtual Voucher fk_DSVoucher { get; set; }
        #endregion
        public string Remark { get; set; }
        public long? ViewId { get; set; } = 0;
        public long? ConsigneeId { get; set; }
        [ForeignKey(nameof(ConsigneeId))]
        public virtual Ledger fk_Consignee { get; set; }
        [Column("DataProps")]
        public string DataProps { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> Data
        {
            //get => _dt==null?(string.IsNullOrWhiteSpace(ExtraProperties)?null: JsonConvert.DeserializeObject<List<JsonDataEntity>>(ExtraProperties)): _dt;
            get
            {
                try
                {
                    if (DataProps == "{}") DataProps = "[]";
                    return _dt ?? (_dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>(DataProps ?? (DataProps = "[]")));
                }
                catch
                {
                    return _dt ?? (_dt = new List<JsonDataEntity>());
                }

            }
            set
            {
                _dt = value;
                DataProps = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }
        }
        public void DeleteAndAdd(JsonDataEntity entity)
        {
            try
            {
                if ((DataProps ?? "{}") == "{}") DataProps = "[]";
                if (_dt == null)
                {
                    _dt = JsonConvert.DeserializeObject<List<JsonDataEntity>>((DataProps ?? (DataProps = "[]")));
                }

                _dt.RemoveAll(x => x.DataName == entity.DataName);
                _dt.Add(entity);
                DataProps = JsonConvert.SerializeObject(_dt);
            }
            catch
            {
                DataProps = "[]";
            }
        }
    }
}
