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
using TrackoApi.Models.Validations;

using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.Models.BMS
{
    [Table("tCNMaster")]
    public class CNMaster : AuditableEntity,IValidatableObject/*,IAprovalEntity*/
    {
        public CNMaster()
        {
            Materials=new List<CNMultiMaterial>();
            MultiMaterialsView=new List<vwCNMultiMaterial>();
            PODDetails = new List<CNExtraInfo>();
            EWayBills = new List<vwEWayBill>();
        }
        [Precision(28, 4)]
        public decimal CurRate { get; set; } = 0;
        public long? CurTypeId { get; set; }
        [ForeignKey("CurTypeId")]
        public virtual GenericMaster fk_CurType { get; set; }

        public long? ConstCurTypeId { get; set; }
        [ForeignKey("ConstCurTypeId")]
        public virtual GenericMaster fk_ConstCurType { get; set; }

        #region Basic Section

        [Index("XI_CNMaster_CNNo",IsUnique = true),MaxLength(30),StationaryCheck]
        public string CNNo { get; set; }
        /// <summary>
        /// CnType: constant typeId=102
        /// </summary>
        public long CNTypeId { get; set; }
        [ForeignKey("CNTypeId")]
        public virtual ConstantValue fk_CNType { get; set; }
        [DataType(DataType.Date)]
        public DateTime CNDate { get; set; }
        public long? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual VehicleMaster fk_Vehicle { get; set; }
        public long? HireVehicleId { get; set; }
        [ForeignKey("HireVehicleId")]
        public virtual HireVehicle fk_HireVehicle { get; set; }

        [Column("ActRouteId")]  
        public long? ActualRouteId { get; set; }
        [ForeignKey("ActualRouteId")]
        public virtual RouteMaster fk_ActualRoute { get; set; }
        [Column("ChgRouteId")]
        public long? ChargedRouteId { get; set; }
        [ForeignKey("ChargedRouteId")]
        public virtual RouteMaster fk_ChargedRoute { get; set; }
        public bool IsZeroFreightCN { get; set; } = false;
        public long? BillId { get; set; }
        [ForeignKey("BillId")]
        public virtual CNBill fk_Bill { get; set; }

        public long? BillingOfficeId { get; set; }
        [ForeignKey("BillingOfficeId")]
        public virtual OfficeMaster fk_Billoffice { get; set; }
        public long? LoadingOfficeId { get; set; }
        [ForeignKey("LoadingOfficeId")]
        public virtual OfficeMaster fk_LoadOffice { get; set; }
        public long? DestinationOfficeId { get; set; }
        [ForeignKey("DestinationOfficeId")]
        public virtual OfficeMaster fk_DesOffice { get; set; }
        public bool IsFreightCalMM { get; set; }
        #endregion

        #region TimeLine

        public DateTime? ETA { get; set; }
        public DateTime? ReachTime { get; set; }
        public DateTime? DeliveryDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? PODDate { get; set; }

        public long? LoadingPointId { get; set; }
        [ForeignKey("LoadingPointId")]
        public virtual CityMaster fk_LoadingPoint { get; set; }

        public long? OffLoadingPointId { get; set; }
        [ForeignKey("OffLoadingPointId")]
        public virtual CityMaster fk_OffLoadingPoint { get; set; }

        #endregion

        #region Client Section

        public long? BillingPartyId { get; set; }
        [ForeignKey("BillingParty")]
        public virtual Ledger fk_BillingParty { get; set; }
        public long? ConsigneeId { get; set; }
        [ForeignKey("ConsigneeId")]
        public virtual Ledger fk_Consignee { get; set; }
        public long? ConsignorId { get; set; }
        [ForeignKey("ConsignorId")]
        public virtual Ledger fk_Consignor { get; set; }

        public long? MaterialId { get; set; }
        [ForeignKey("MaterialId")]
        public virtual MaterialMaster fk_Material { get; set; }
        [MaxLength(300)]
        public string MaterialInfo { get; set; }
        [MaxLength(300)]
        public string InvoiceNo { get; set; }
        [DataType(DataType.Date)]
        public DateTime? InvoiceDate { get; set; }
        public long? Ref1Id { get; set; }
        [ForeignKey("Ref1Id")]
        public virtual GenericMaster fk_Ref1 { get; set; }
        public long? Ref2Id { get; set; }
        [ForeignKey("Ref2Id")]
        public virtual GenericMaster fk_Ref2 { get; set; }
        public long? Ref3Id { get; set; }
        [ForeignKey("Ref3Id")]
        public virtual GenericMaster fk_Ref3 { get; set; }
        public long? Ref4Id { get; set; }
        [ForeignKey("Ref4Id")]
        public virtual GenericMaster fk_Ref4 { get; set; }
        [MaxLength(300)]
        public string RefI { get; set; }
        [MaxLength(300)]
        public string RefII { get; set; }
        [MaxLength(300)]
        public string RefIII { get; set; }
        [MaxLength(300)]
        public string RefIV { get; set; }
        [MaxLength(300)]
        public string RefV { get; set; }
        #endregion

        #region Qty, Rate,Unit Section
        [Precision(28, 5)]
        public decimal KM { get; set; } = 0;

        [Precision(28, 10)]
        public decimal ActualWeight { get; set; } = 0;

        [Column("ActWtUnitId")]
        public long? ActualWeightUnitId { get; set; }

        [ForeignKey("ActualWeightUnitId")]
        public virtual UnitMaster fk_ActualWeightUnit { get; set; }

        [Precision(28, 10)]
        public decimal ChargedWeight { get; set; } = 0;

        [Column("ChgWtUnitId")]
        public long? ChargedWeightUnitId { get; set; }
        [ForeignKey("ChargedWeightUnitId")]
        public virtual UnitMaster fk_ChargedWeightUnit { get; set; }

        public decimal Load20cQty { get; set; } = 0;
        public decimal OffLoad20cQty { get; set; } = 0;

        public decimal ActualQty { get; set; } = 0;

        [Column("ActQtyUnitId")]
        public long? ActualQtyUnitId { get; set; }

        [ForeignKey("ActualQtyUnitId")]
        public virtual UnitMaster fk_ActualQtyUnit { get; set; }

        public decimal ChargedQty { get; set; } = 0;

        [Column("ChgQtyUnitId")]
        public long? ChargedQtyUnitId { get; set; }

        [ForeignKey("ChargedQtyUnitId")]
        public virtual UnitMaster fk_ChargedQtyUnit { get; set; }
        /// <summary>
        /// Gets or sets Total Package for Actual Qty.
        /// </summary>
        /// <value>Total Package.</value>
        public decimal TotalPackage { get; set; } = 0;

        public long? PkgUnitId { get; set; }
        [ForeignKey("PkgUnitId")]
        public virtual UnitMaster fk_PkgUnit { get; set; }

        public long? TransportModeId { get; set; }
        [ForeignKey("TransportModeId")]
        public virtual ConstantValue fk_TransportMode { get; set; }

        #endregion Section

        #region Freight & Other Charges Section

        public long? LoadTypeId { get; set; }
        [ForeignKey("LoadTypeId")]
        public virtual LoadType fk_LoadType { get; set; }
        /// <summary>
        /// Constant Id 1164
        /// </summary>
        public long? VehicleTypeId { get; set; }
        [ForeignKey("VehicleTypeId")]
        public virtual GenericMaster fk_VehicleType { get; set; }

        public long? ContractId { get; set; }
        [ForeignKey("ContractId")]
        public virtual CNRateContract fk_Contract { get; set; }

        public long? RateId { get; set; }
        [ForeignKey("RateId")]
        public virtual CNRateContractLog fk_Rate { get; set; }

        public long? TaxServiceTypeId { get; set; }
        [ForeignKey("TaxServiceTypeId")]
        public virtual TaxServiceType fk_TaxServiceType { get; set; }

        #region Basic Freight
        [Precision(28, 10)]
        public decimal Rate { get; set; } = 0;
        [Precision(28,5)]
        public decimal CNBasicAmount { get; set; } = 0;
        public decimal DiscPercent { get; set; } = 0;
        [Precision(28, 10)]
        public decimal Discount { get; set; } = 0;
        [Precision(28, 10)]
        public decimal CNSubTotalI { get; set; } = 0;

        #endregion
        #region Addition/Less [Gross Freight]
        [Precision(28, 7)]
        public decimal NTAdd1 { get; set; } = 0;
        [Precision(28, 7)]
        public decimal NTAdd2 { get; set; } = 0;
        [Precision(28, 7)]
        public decimal NTAdd3 { get; set; } = 0;

        public decimal LDetentionRate { get; set; } = 0;
        public decimal LDetentionDays { get; set; } = 0;

        public decimal ULDetentionRate { get; set; } = 0;
        public decimal ULDetentionDays { get; set; } = 0;

        public decimal LDPenaltyRate { get; set; } = 0;
        public decimal LDPenaltyDays { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeI { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeII { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeIII { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeIV { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeV { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeVI { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeVII { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeVIII { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeIX { get; set; } = 0;
        [Precision(28, 10)]
        public decimal AChargeX { get; set; } = 0;
        [Precision(28, 10)]
        public decimal LChargeI { get; set; } = 0;
        [Precision(28, 10)]
        public decimal LChargeII { get; set; } = 0;
        [Precision(28, 10)]
        public decimal LChargeIII { get; set; } = 0;
        [Precision(28, 10)]
        public decimal LChargeIV { get; set; } = 0;
        [Precision(28, 10)]
        public decimal CNSubTotalII { get; set; } = 0;
        #endregion
        #region Net Freight
        public decimal IServiceTax { get; set; } = 0;
        public long? ServiceTaxPaidById { get; set; }
        [ForeignKey("ServiceTaxPaidById")]
        public virtual ConstantValue fk_ServiceTaxPaidBy { get; set; }

        public decimal IGSTRate { get; set; } = 0;

        [Precision(28, 10)]
        public decimal IGSTAmount { get; set; } = 0;
        public decimal CGSTRate { get; set; } = 0;

        [Precision(28, 10)]
        public decimal CGSTAmount { get; set; } = 0;
        public decimal SGSTRate { get; set; } = 0;

        [Precision(28, 10)]
        public decimal SGSTAmount { get; set; } = 0;

        [Precision(28, 10)]
        public decimal CNTotalFreight { get; set; } = 0;
        public decimal OServiceTax { get; set; } = 0;
        public decimal ODeliveryST { get; set; } = 0;
        [Precision(28, 10)]
        public decimal PreviousTotalFreight { get; set; } = 0;
        #endregion
        public long? TripLogId { get; set; }
        [ForeignKey("TripLogId")]
        public virtual VehicleMovementLog fk_TripLog { get; set; }
        /// <summary>
        /// Gets or sets the tl load qty.
        /// <remarks>Qty Loaded in Above TripLog</remarks>
        /// </summary>
        /// <value>The tl load qty.</value>
        public decimal TLLoadQty { get; set; }
        #endregion

        #region OtherInfo
        [MaxLength(300)]
        public string PrivateMarka { get; set; }
        public decimal ValueofGoods { get; set; }

        [MaxLength(1000)]
        public string Remark { get; set; }

        public DateTime? BalancePayDate { get; set; }
        #endregion
        [Column("Temperature")]
        public decimal Temperature { get; set; } = 0;
        [Column("Density")]
        [Precision(18, 3)]
        public decimal Density { get; set; } = 0;
        [Column("CorrectionFactor")]
        public decimal CorrectionFactor { get; set; } = 0;

        public virtual List<CNBillLog> BillLogs { get; set; }
        public virtual List<CNStockLog> StockLogs { get; set; }
        public virtual List<CnStatusLog> CnStatusLogs { get; set; }
        public virtual List<CNMultiMaterial> Materials { get; set; }
        public virtual List<CNDTSStatusLog> DTSStatusLogs { get; set; }
        [Column("IsBilled")]
        public bool CreateBillOnCNCreate { get; set; }

        public long? ViewId { get; set; }
        #region Add/Less Remarks
        public long? AddRemark1Id { get; set; }
        [ForeignKey("AddRemark1Id")]
        public virtual GenericMaster fk_AddRemark1 { get; set; }
        public long? AddRemark2Id { get; set; }
        [ForeignKey("AddRemark2Id")]
        public virtual GenericMaster fk_AddRemark2 { get; set; }
        public long? AddRemark3Id { get; set; }
        [ForeignKey("AddRemark3Id")]
        public virtual GenericMaster fk_AddRemark3 { get; set; }
        public long? AddRemark4Id { get; set; }
        [ForeignKey("AddRemark4Id")]
        public virtual GenericMaster fk_AddRemark4 { get; set; }
        public long? AddRemark5Id { get; set; }
        [ForeignKey("AddRemark5Id")]
        public virtual GenericMaster fk_AddRemark5 { get; set; }
        public long? AddRemark6Id { get; set; }
        [ForeignKey("AddRemark6Id")]
        public virtual GenericMaster fk_AddRemark6 {get; set;}

        public long? LessRemark1Id { get; set; }
        [ForeignKey("LessRemark1Id")]
        public virtual GenericMaster fk_LessRemark1 { get; set; }

        public long? LessRemark2Id { get; set; }
        [ForeignKey("LessRemark2Id")]
        public virtual GenericMaster fk_LessRemark2 { get; set; }

        public long? LessRemark3Id { get; set; }
        [ForeignKey("LessRemark3Id")]
        public virtual GenericMaster fk_LessRemark3 { get; set; }

        public long? LessRemark4Id { get; set; }
        [ForeignKey("LessRemark4Id")]
        public virtual GenericMaster fk_LessRemark4 { get; set; }

        #endregion

        #region Add & Less Remarks Text field
        [MaxLength(500)]
        public string Add1CNRemarks { get; set; }
        [MaxLength(500)]
        public string Add2CNRemarks { get; set; }
        [MaxLength(500)]
        public string Add3CNRemarks { get; set; }
        [MaxLength(500)]
        public string Add4CNRemarks { get; set; }
        [MaxLength(500)]
        public string Add5CNRemarks { get; set; }
        [MaxLength(500)]
        public string Add6CNRemarks { get; set; }
        [MaxLength(500)]
        public string Less1CNRemarks { get; set; }
        [MaxLength(500)]
        public string Less2CNRemarks { get; set; }
        [MaxLength(500)]
        public string Less3CNRemarks { get; set; }
        [MaxLength(500)]
        public string Less4CNRemarks { get; set; }
        #endregion
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //var container = GlobalConfiguration.Configuration.DependencyResolver
            //    .GetService(typeof(IUnityContainer)) as IUnityContainer;
            //var userService = container.Resolve<IEntityTable<StationeryBookLog>>();
            //if (userService == null)
            //{
            //    Debug.WriteLine("Stationary Service was Null");
            //}

            if (this.TLLoadQty > this.ActualQty)
            {
                yield return new ValidationResult($"TL loaded Qty {this.TLLoadQty} cannot be greater than CN Actual Qty {this.ActualQty}");
            }
            if (LoadingOfficeId.GetValueOrDefault(0)==0)
            {
                yield return new ValidationResult("Loading Office is required");
            }
            if (BillingOfficeId.GetValueOrDefault() == 0 && LoadingOfficeId > 0)
            {
                BillingOfficeId = LoadingOfficeId;
            }
            //if (string.IsNullOrWhiteSpace(CNNo) && AutoStationar .GetValueOrDefault(0) == 0)
            //{
            //    yield return new ValidationResult("CN Number is required");
            //}
        }
        [MaxLength(100)]
        public string BatchId { get; set; }
        public virtual List<vwCNMultiMaterial> MultiMaterialsView { get; set; }
        public virtual List<vwEWayBill> EWayBills { get; set; }
        public virtual List<CNExtraInfo> PODDetails { get; set; }
        public long? DeliveryTypeId { get; set; }
        public decimal CnAdvance { get; set; } = 0;
        public long? CnAdvanceId { get; set; }
        [ForeignKey("CnAdvanceId")]
        public virtual CNBillPayment fk_CnAdvanceId { get; set; }

        public long? OrderRequestId { get; set; }
        [ForeignKey("OrderRequestId")]
        public virtual SalesOrderRequest fk_OrderRequest { get; set; }

        public bool IsTaxApplicable { get; set; } = true;
        public bool IsApproved { get; set; } = false;
        //public DateTime? APRLDateTime { get; set; }
        //public string APRLRemark { get; set; }
        //public long? APRLSID { get; set; }
        //public long? APRLUserId { get; set; }
        //public bool IsAutoAPRL { get; set; } = false;

        [MaxLength(1000)]
        public string PODRemark { get; set; }

        [MaxLength(150)]
        public string EWayBillCN { get; set; }
        public DateTime? eWayBillValidity { get; set; }
        public string JsonData { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> JsonDataList
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
        public IDictionary<string, object> DynamicProperties { get; set; }
        public void PrepareSalesLog(ref SalesLog log)
        {
            if(CNTotalFreight==0 && log.Id > 0)
            {
                log.ObjectState = ObjectState.Deleted;
                return;
            }
            log.AChargeI = AChargeI;
            log.DocDate = CNDate;
            log.AChargeII = AChargeII;
            log.AChargeIII = AChargeIII;
            log.AChargeIV = AChargeIV;
            log.AChargeIX = AChargeIX;
            log.AChargeV = AChargeV;
            log.AChargeVI = AChargeVI;
            log.AChargeVII = AChargeVII;
            log.AChargeVIII = AChargeVIII;
            log.AChargeX = AChargeX;
            log.ActualQty = ActualQty;
            log.ActualQtyUnitId = ActualQtyUnitId;
            log.ActualWeight = ActualWeight;
            log.ActualWeightUnitId = ActualWeightUnitId;
            log.BasicFreight = CNBasicAmount;
            log.BillingOfficeId = BillingOfficeId;
            log.BillingPartyId = BillingPartyId;
            log.CGSTAmount = CGSTAmount;
            log.CGSTRate = CGSTRate;
            log.CNId = Id;
            log.ChargeQty = ChargedQty;
            log.ChargeQtyUnitId = ChargedQtyUnitId;
            log.ChargeWeight = ChargedWeight;
            log.ChargeWeightUnitId = ChargedWeightUnitId;
            log.DiscPercent = DiscPercent;
            log.Discount = Discount;
            log.DocNo = CNNo;
            log.GSTPaidById = ServiceTaxPaidById;
            log.GSTServiceTypeId = TaxServiceTypeId;
            log.IGSTAmount = IGSTAmount;
            log.IGSTRate = IGSTRate;
            log.IsTaxApplicable = IsTaxApplicable;
            log.LChargeI = LChargeI;
            log.LChargeII = LChargeII;
            log.LChargeIII = LChargeIII;
            log.LChargeIV = LChargeIV;
            log.LChargeIX = 0;
            log.LChargeV = 0;
            log.LChargeVI = 0;
            log.LChargeVII = 0;
            log.LChargeVIII = 0;
            log.LChargeX = 0;
            log.LDPenaltyDays = LDPenaltyDays;
            log.LDPenaltyRate = LDPenaltyRate;
            log.LDetentionDays = LDetentionDays;
            log.LDetentionRate = LDetentionRate;
            log.Rate = Rate;
            log.NetFreight = CNTotalFreight;
            log.RateChartId = ContractId;
            log.RateId = RateId;
            log.RouteId = ChargedRouteId;
            log.SGSTAmount = SGSTAmount;
            log.GrossFreight = CNSubTotalII;
            log.SGSTRate = SGSTRate;
            log.SubTotal = CNSubTotalI;
            log.SalesOfficeId = LoadingOfficeId;
            log.ULDetentionDays = ULDetentionDays;
            log.ULDetentionRate = ULDetentionRate;
            log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added;
        }
    }
}