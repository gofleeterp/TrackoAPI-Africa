using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.BMS;

namespace TrackoApi.Models.BMS
{
    [Table("tCNChallan")]
    public class CnChallan : AuditableEntity,IValidatableObject
    {
        private readonly XmlSerializeDeserialize<List<vwCNStockMMLog>> _serializeDeserialize;
        private List<vwCNStockMMLog> _tempCNStockMMLogs;

        public DateTime? ShipmentDate { get; set; }
        public CnChallan()
        {
            ObjectState = ObjectState.Unchanged;
            _serializeDeserialize = new XmlSerializeDeserialize<List<vwCNStockMMLog>>();
        }
        /// <summary>
        /// Gets or sets the log type identifier.
        /// <remarks>
        /// Options StockIn,StockOut,Expected and Delivered
        /// ConstantTypeId 108
        /// </remarks>
        /// </summary>
        /// <value>The log type identifier.</value>
        public long? LogTypeId { get; set; }
        [ForeignKey("LogTypeId")]
        public ConstantValue fk_LogType { get; set; }

        public bool IsDeliveryFailed { get; set; } = false;
        public DateTime? DeliveryFailedDate { get; set; }
        [Column("CNId"), ForeignKey("fk_CNMaster"), Required, Index("IX_CnChallan_CNID_TLID", IsUnique = true, Order = 1)]
        public long CNId { get; set; }
        public virtual CNMaster fk_CNMaster { get; set; }
        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }
        public long? OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public virtual OfficeMaster fk_Office { get; set; }

        [Column("TriplogId"), ForeignKey("fk_Triplog"), Index("IX_CnChallan_CNID_TLID", IsUnique = true, Order = 2)]
        public long? TriplogId { get; set; }
        public virtual VehicleMovementLog fk_Triplog { get; set; }

        [Column("ChallanId"), ForeignKey("fk_Challan"), Index("IX_CnChallan_CNID_TLID", IsUnique = true, Order = 3)]
        public long? ChallanId { get; set; }
        public virtual ChallanMaster fk_Challan { get; set; }
        public decimal Qty { get; set; } = 0;
        public decimal ArrivalQty { get; set; }
        public decimal Excess { get; set; } = 0;
        public decimal Damaged { get; set; } = 0;
        public decimal Short { get; set; } = 0;
        public decimal MarketFreight { get; set; } = 0;
        public decimal Revenue { get; set; } = 0;
        [Column("ActualWeight")]
        public decimal Weight { get; set; }
        [Index("IX_CnChallan_CNID_TLID", IsUnique = true, Order = 4)]
        public long? RefStockId { get; set; }
        [ForeignKey("RefStockId")]
        public virtual CNStockLog fk_RefStockLog { get; set; }
        public virtual CnChallanCharges fk_CnChallanCnCharges { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? UnloadDate { get; set; }
        public string ArrivalRemark { get; set; }
        public virtual List<CNStockLog> CnStockLogs { get; set; }
        public virtual List<CNStockMMLog> CnMMLogs { get; set; }

        public List<vwCNStockMMLog> tempCNStockMMLogs { get => _tempCNStockMMLogs; set  {
                _tempCNStockMMLogs = value;
                _cnMMXml = value == null || value.Count == 0 ? null : _serializeDeserialize.SerializeData(value);
            } }
        //{
        //    get
        //    {
        //        if (_tempCNStockMMLogs!=null&&_tempCNStockMMLogs.Any()) return _tempCNStockMMLogs;
        //        return string.IsNullOrWhiteSpace(_cnMMXml)
        //            ? new List<vwCNStockMMLog>()
        //            : _serializeDeserialize.DeserializeData(_cnMMXml);
        //    }
        //    set
        //    {
        //        _tempCNStockMMLogs = value;
        //        _cnMMXml =value==null||value.Count==0 ?null: _serializeDeserialize.SerializeData(value);
        //    }
        //}

        public long? ViewId { get; set; }

        [MaxLength(200)]
        public string Ref1 { get; set; }
        /// <summary>
        /// ConstantId 1472 and 1545
        /// </summary>
        public long? DeliveryTypeId { get; set; }
        [ForeignKey("DeliveryTypeId")]
        public virtual ConstantValue fk_DeliveryType { get; set; }
        [XmlSqlType, IgnoreDataMember]
        public string _cnMMXml { get; set; }

        public long? ArrivalViewId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.UnloadDate == null && this.ArrivalDate != null)
            {
                this.UnloadDate = this.ArrivalDate;
            }else if(this.UnloadDate != null && this.ArrivalDate == null)
            {
                this.ArrivalDate = this.UnloadDate;
            }
            if (CNId==0)
            {
                yield return new ValidationResult("CN Should be Attached for Creating CN Challan");
            }
        }
    }
    /// <summary>
    /// Capture the Charges Charged while CNWas Loaded or Unloaded or while CN Was EnRoute
    /// </summary>
    [Table("tCNChallanCharges")]
    public class CnChallanCharges : Base.Entity
    {
        [Column("Id"), Key, ForeignKey("fk_CnChallan")]
        public override long Id { get; set; }
        public virtual CnChallan fk_CnChallan { get; set; }

        [Column("CNId"), ForeignKey("fk_CNMaster"), Required]
        public long CNId { get; set; }
        public virtual CNMaster fk_CNMaster { get; set; }
        public int DetentionDays { get; set; } = 0;
        public decimal DetentionRate { get; set; } = 0;
        public decimal Detention { get; set; } = 0;
        
        public decimal AddChg4 { get; set; } = 0;
        public decimal UnloadCharges { get; set; } = 0;
        public decimal Penalty { get; set; } = 0;
        public decimal Claims { get; set; } = 0;
        public decimal AddChg1 { get; set; } = 0;
        public decimal AddChg2 { get; set; } = 0;
        public decimal AddChg3 { get; set; } = 0;
        public decimal LessChg1 { get; set; } = 0;
        public decimal LessChg2 { get; set; } = 0;
        public decimal LessChg3 { get; set; } = 0;
        public decimal LessChg4 { get; set; } = 0;
    }
}