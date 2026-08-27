using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS.Inventory
{
    [Table("tPurchaseOrder")]
    public class PurchaseOrder:AuditableEntity
    {
        /// <summary>
        /// potype come from constant with constant type=129
        /// </summary>
        [Column("TypeId"), Required]
        public long TypeId { get; set; }
        [ForeignKey("TypeId")]
        public virtual ConstantValue fk_Type { get; set; }

        [Column("OfficeId"), Required]
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("PONo"), StationaryCheck, Required,MaxLength(150)]
        public string PONo { get; set; }

        [Column("PODate"), Required]
        public DateTime PODate { get; set; }

        [Column("VendorId"), Required]
        public long VendorId { get; set; }
        [ForeignKey("VendorId")]
        public virtual Ledger fk_Vendor { get; set; }

      
        /// <summary>
        /// usage point come from constant with constant type=130
        /// usage point: Tyre PO, Spare PO, Workorder
        /// </summary>
        public long? UsagePointId { get; set; }
        [ForeignKey("UsagePointId")]
        public virtual ConstantValue fk_UsagePoint { get; set; }


        /// <summary>
        /// nature:PO Nature like "Labour","BodyWork","Cleaning"
        /// specially used in case of labour PO
        /// </summary>
        public long? NatureId { get; set; }
        [ForeignKey("NatureId")]
        public virtual GenericMaster fk_Nature { get; set; }


        [Column("IsCancelled")]
        public bool? IsCancelled { get; set; }

        //[Column("ClosingDate")]
        //public DateTime? ClosingDate { get; set; }

        [Column("ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [Column("PreClosingDate")]
        public DateTime? PreClosingDate { get; set; }

        [Column("POValue")]
        public decimal POValue { get; set; } = 0;

        public DateTime? CancelDate { get; set; }

        public long? CancelPOId { get; set; }
        [ForeignKey("CancelPOId")]
        public virtual PurchaseOrder fk_CancelPO { get; set; }

        [Column("Remarks")]
        [MaxLength(1000)]
        public string Remarks { get; set; }

        [Column("StatusId")]
        public long StatusId { get; set; }
        public long? ViewId { get; set; }

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        public decimal FreightChg { get; set; } = 0;
        public decimal FreightVATP { get; set; } = 0;
        public decimal FreightVATAmount { get; set; } = 0;

        public virtual List<PurchaseOrderLog> Logs { get; set; }
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