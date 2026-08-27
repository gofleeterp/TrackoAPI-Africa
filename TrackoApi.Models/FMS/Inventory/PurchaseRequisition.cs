using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tPR")]
    public class PurchaseRequisition : AuditableEntity
    {
        public PurchaseRequisition()
        {
            Logs = new List<PurchaseRequisitionLog>();
        }
        [Column("DocDate"), Required]
        public DateTime DocDate { get; set; }
        public DateTime? FulFillmentDate { get; set; }

        [Column("DocNo"), StationaryCheck, Required, MaxLength(100), MinLength(3), Index("IX_PurchaseRequition_DocNo", IsUnique = true)]
        public string DocNo { get; set; }

        [Column("OfficeId"), Required, ForeignKey("fk_Office")]
        public long OfficeId { get; set; }
        public virtual OfficeMaster fk_Office { get; set; }

        

        [Column("StoreId"), ForeignKey("fk_Store")]
        public long? StoreId { get; set; }
        public virtual Ledger fk_Store { get; set; }
 
        [Column("Remarks")]
        [MaxLength(2500)]
        public string Remarks { get; set; }

        public long StatusId { get; set; }
        [ForeignKey("StatusId")]
        public virtual ConstantValue fk_Status { get; set; }

        public virtual ICollection<PurchaseRequisitionLog> Logs { get; set; }

        public long? ViewId { get; set; }
        public string LogsJson { get; set; }
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
