using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.BMS
{
    [Table("tHMArrival")]
    public class HMArrival : AuditableEntity
    {
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        public HMArrival()
        {
            ObjectState = ObjectState.Added;
        }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }
        public long TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }

        public long? HirePartyId { get; set; }
        [ForeignKey("HirePartyId")]
        public virtual Ledger fk_HireParty { get; set; }

        public decimal TDSRate { get; set; }
        public decimal TDSAmount { get; set; }
        public long? TDSAccountId { get; set; }
        [ForeignKey("TDSAccountId")]
        public virtual Ledger fk_TDSAccount { get; set; }
        public long? TDSVoucherId { get; set; }
        [ForeignKey("TDSVoucherId")]
        public virtual Voucher fk_TDSVoucher { get; set; }

        public decimal TaxableAmount { get; set; }
        public long? TaxableAmtVoucherId { get; set; }
        [ForeignKey("TaxableAmtVoucherId")]
        public virtual Voucher fk_TaxableAmtVoucher { get; set; }

        public decimal NonTaxAmount { get; set; }
        public long? NonTaxAmtVoucherId { get; set; }
        [ForeignKey("NonTaxAmtVoucherId")]
        public virtual Voucher fk_NonTaxAmtVoucher { get; set; }

        public decimal NetAmount { get; set; }
        [MaxLength(1000)]
        public string Remark { get; set; }
        public virtual List<HMArrivalLog> ArrivalLogs { get; set; }
        [MaxLength(150)]
        public string BatchId { get; set; }
        public long? ViewId { get; set; }

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
