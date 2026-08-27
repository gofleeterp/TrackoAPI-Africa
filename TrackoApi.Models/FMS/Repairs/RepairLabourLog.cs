using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.FMS
{
    [Table("tRepairLabourLog")]
    public class RepairLabourLog:AuditableEntity
    {
        public long? TSLId { get; set; }
        [ForeignKey("TSLId")]
        public virtual TransactionSupportLog fk_TSL { get; set; }

        public long? VoucherId { get; set; }
        [ForeignKey("VoucherId")]
        public virtual Voucher fk_Voucher { get; set; }

        public long? ExtraInfoId { get; set; }
        [ForeignKey("ExtraInfoId")]
        public virtual SpareLogExtraInfo ExtraInfo { get; set; }
        [Column("JobCardId"), ForeignKey("fk_JobCard")]
        public long? JobCardId { get; set; }
        public virtual VehicleMovementLog fk_JobCard { get; set; }
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }

        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }

        // public long? PurchaseOrderId { get; set; }
        // [ForeignKey("PurchaseOrderId")]
        // public virtual PurchaseOrder fk_PurchaseOrder { get; set; }
        [Column("POLogId")]
        public long? POLogId { get; set; }
        [ForeignKey(nameof(POLogId))]
        public PurchaseOrderLog fk_POLog { get; set; }
        public long LaborId { get; set; }
        [ForeignKey("LaborId")]
        public virtual SpareMaster fk_Labor { get; set; }
        public long? MechanicId { get; set; }
        [ForeignKey("MechanicId")]
        public virtual GenericMaster fk_Mechanic { get; set; }
        public decimal LaborQty { get; set; } = 0;
        public long? LaborUnitId { get; set; }
        [ForeignKey("LaborUnitId")]
        public virtual UnitMaster fk_LaborUnit { get; set; }
        public decimal LaborRate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public decimal DiscountPercent { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        //public decimal ServiceTaxPercent { get; set; } = 0;
        //public decimal ServiceTaxAmount { get; set; } = 0;

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        public decimal CGSTPercent { get; set; } = 0;
        public decimal CGSTAmount { get; set; } = 0;

        public decimal SGSTPercent { get; set; } = 0;
        public decimal SGSTAmount { get; set; } = 0;

        public decimal IGSTPercent { get; set; } = 0;
        public decimal IGSTAmount { get; set; } = 0;

        public int ODOKm { get; set; }
        public decimal SubTotal { get; set; } = 0;
        public decimal OtherAmount { get; set; } = 0;
        public decimal NetAmount { get; set; } = 0;
        [MaxLength(500)]
        public string Remark { get; set; }
        [MaxLength(200)]
        public string BatchId { get; set; }
        public decimal Value1 { get; set; } = 0;
        public decimal Value2 { get; set; } = 0;

        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        [Precision(28, 4)] 
        public decimal CurRate { get; set; } = 0;
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        public void Compute()
        {
            Amount = LaborRate*LaborQty;
            //SubTotal = Amount - DiscountAmount + CGSTAmount + SGSTAmount + IGSTAmount;
            //NetAmount = SubTotal + OtherAmount;
        }
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
