using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;

namespace TrackoApi.Models.BMS
{
    [Table("mCNRateContractLog")]
    public class CNRateContractLog : AuditableEntity
    {
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }
        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public long RateContractId { get; set; }
        [ForeignKey("RateContractId")]
        public virtual CNRateContract fk_RateContract { get; set; }
        public long? ScriptId { get; set; }
        [ForeignKey("ScriptId")]
        public virtual ApiWorkFlowScript ApiWorkFlowScript { get; set; }
        public long? LoadTypeId { get; set; }
        [ForeignKey("LoadTypeId")]
        public virtual LoadType fk_LoadType { get; set; }

        /// <summary>
        /// Constant Id 1164
        /// </summary>
        public long? VehicleTypeId { get; set; }
        [ForeignKey("VehicleTypeId")]
        public virtual GenericMaster fk_VehicleType { get; set; }

        public long? ConsigneeId { get; set; }
        [ForeignKey("ConsigneeId")]
        public virtual Ledger fk_Consignee { get; set; }

        public long? ConsignorId { get; set; }
        [ForeignKey("ConsignorId")]
        public virtual Ledger fk_Consignor { get; set; }

        public long? StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual GenericMaster fk_State { get; set; }

        public long? RouteId { get; set; }
        [ForeignKey("RouteId")]
        public virtual RouteMaster fk_Route { get; set; }
        public long? MaterialgroupId { get; set; }
        [ForeignKey("MaterialgroupId")]
        public virtual MaterialGroup fk_MaterialGroup { get; set; }
        [Precision(28, 5)]
        public decimal? Km { get; set; }
        [Precision(28, 10)]
        public decimal Rate { get; set; } = 0;
        /// <summary>
        /// Gets or sets the fixed freight.
        /// <remarks>A fix Freight to added after calculating Freight as per Load type Factor</remarks>
        /// </summary>
        /// <value>The fixed freight.</value>
        [Precision(28, 10)]
        public decimal FixedFreight { get; set; }
        /// <summary>
        /// Gets or sets the base value.
        /// <remarks>Base factor Qty</remarks>
        /// </summary>
        /// <value>The base value.</value>
        [Precision(28, 10)]
        public decimal BaseFactorQty { get; set; }
        [Precision(28, 10)]
        public decimal MinValue { get; set; } = 0;
        [Precision(28, 10)]
        public decimal MaxValue { get; set; } = 0;
        [Precision(28, 10)]
        public decimal Discount { get; set; } = 0;
        /// <summary>
        /// constant GroupId=104
        /// </summary>
        public long? DisFactorId { get; set; }
        [ForeignKey("DisFactorId")]
        public virtual ConstantValue fk_DisFactor { get; set; }

        [Precision(28, 10)]
        public decimal A1Charge { get; set; } = 0;
        public long? A1FactorId { get; set; }
        [ForeignKey("A1FactorId")]
        public virtual ConstantValue fk_A1Factor { get; set; }
        [Precision(28, 10)]
        public decimal A2Charge { get; set; } = 0;
        public long? A2FactorId { get; set; }
        [ForeignKey("A2FactorId")]
        public virtual ConstantValue fk_A2Factor { get; set; }
        [Precision(28, 10)]
        public decimal A3Charge { get; set; } = 0;
        public long? A3FactorId { get; set; }
        [ForeignKey("A3FactorId")]
        public virtual ConstantValue fk_A3Factor { get; set; }
        [Precision(28, 10)]
        public decimal A4Charge { get; set; } = 0;
        public long? A4FactorId { get; set; }
        [ForeignKey("A4FactorId")]
        public virtual ConstantValue fk_A4Factor { get; set; }
        [Precision(28, 10)]
        public decimal A5Charge { get; set; } = 0;
        public long? A5FactorId { get; set; }
        [ForeignKey("A5FactorId")]
        public virtual ConstantValue fk_A5Factor { get; set; }
        [Precision(28, 10)]
        public decimal A6Charge { get; set; } = 0;
        public long? A6FactorId { get; set; }
        [ForeignKey("A6FactorId")]
        public virtual ConstantValue fk_A6Factor { get; set; }
        [Precision(28, 10)]
        public decimal A7Charge { get; set; } = 0;
        public long? A7FactorId { get; set; }
        [ForeignKey("A7FactorId")]
        public virtual ConstantValue fk_A7Factor { get; set; }
        [Precision(28, 10)]
        public decimal A8Charge { get; set; } = 0;
        public long? A8FactorId { get; set; }
        [ForeignKey("A8FactorId")]
        public virtual ConstantValue fk_A8Factor { get; set; }
        [Precision(28, 10)]
        public decimal A9Charge { get; set; } = 0;
        public long? A9FactorId { get; set; }
        [ForeignKey("A9FactorId")]
        public virtual ConstantValue fk_A9Factor { get; set; }
        [Precision(28, 10)]
        public decimal A10Charge { get; set; } = 0;
        public long? A10FactorId { get; set; }
        [ForeignKey("A10FactorId")]
        public virtual ConstantValue fk_A10Factor { get; set; }
        [Precision(28, 10)]
        public decimal A11Charge { get; set; } = 0;
        public long? A11FactorId { get; set; }
        [ForeignKey("A11FactorId")]
        public virtual ConstantValue fk_A11Factor { get; set; }
        [Precision(28, 10)]
        public decimal A12Charge { get; set; } = 0;
        public long? A12FactorId { get; set; }
        [ForeignKey("A12FactorId")]
        public virtual ConstantValue fk_A12Factor { get; set; }
        [Precision(28, 10)]
        public decimal A13Charge { get; set; } = 0;
        public long? A13FactorId { get; set; }
        [ForeignKey("A13FactorId")]
        public virtual ConstantValue fk_A13Factor { get; set; }
        [Precision(28, 10)]
        public decimal A14Charge { get; set; } = 0;
        public long? A14FactorId { get; set; }
        [ForeignKey("A14FactorId")]
        public virtual ConstantValue fk_A14Factor { get; set; }
        [Precision(28, 10)]
        public decimal A15Charge { get; set; } = 0;
        public long? A15FactorId { get; set; }
        [ForeignKey("A15FactorId")]
        public virtual ConstantValue fk_A15Factor { get; set; }
        [Precision(28, 10)]
        public decimal A16Charge { get; set; } = 0;
        public long? A16FactorId { get; set; }
        [ForeignKey("A16FactorId")]
        public virtual ConstantValue fk_A16Factor { get; set; }
        [Precision(28, 10)]
        public decimal A17Charge { get; set; } = 0;
        public long? A17FactorId { get; set; }
        [ForeignKey("A17FactorId")]
        public virtual ConstantValue fk_A17Factor { get; set; }

        //Less
        [Precision(28, 10)]
        public decimal L1Charge { get; set; } = 0;
        public long? L1FactorId { get; set; }
        [ForeignKey("L1FactorId")]
        public virtual ConstantValue fk_L1Factor { get; set; }
        [Precision(28, 10)]
        public decimal L2Charge { get; set; } = 0;
        public long? L2FactorId { get; set; }
        [ForeignKey("L2FactorId")]
        public virtual ConstantValue fk_L2Factor { get; set; }
        [Precision(28, 10)]
        public decimal L3Charge { get; set; } = 0;
        public long? L3FactorId { get; set; }
        [ForeignKey("L3FactorId")]
        public virtual ConstantValue fk_L3Factor { get; set; }
        [Precision(28, 10)]
        public decimal L4Charge { get; set; } = 0;
        public long? L4FactorId { get; set; }
        [ForeignKey("L4FactorId")]
        public virtual ConstantValue fk_L4Factor { get; set; }

        [MaxLength(300)]
        public string Ref1 { get; set; }
        [MaxLength(300)]
        public string Ref2 { get; set; }
        [MaxLength(300)]
        public string Ref3 { get; set; }
        [MaxLength(300)]
        public string Ref4 { get; set; }
        public MasterStatus Status { get; set; }

        /// <summary>
        /// //added by sanjay
        /// </summary>
        public long? MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual MaterialMaster fk_Material { get; set; }
        public long? TripModeId { get; set; }
        [ForeignKey("TripModeId")]
        public virtual GenericMaster fk_TripMode { get; set; }
        public decimal TAT { get; set; } = 0;

        [MaxLength(100)]
        public string BatchId { get; set; }

    }
}