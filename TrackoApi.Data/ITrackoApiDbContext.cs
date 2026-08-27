using System;
using System.Data.Entity;
using Repository.Pattern.DataContext;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.FMS.Loan;
using TrackoApi.Models.FMS.Repairs;
using TrackoApi.Models.FMS.Tyres;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.DTS;
using TrackoAPI.Reporting.Models;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Models.CRM;

namespace TrackoApi.Data
{
    /// <exclude />
    public interface ITrackoApiDbContext : IDataContextAsync
    {

        string ToString();
        bool Equals(object obj);
        int GetHashCode();
        Type GetType();
        T Attach<T>(T value) where T : class;

        //20151127: start FMS
        DbSet<GeneralTransaction> GeneralTransactions { get; set; }
        DbSet<GeneralTransLog> GeneralTransLogs { get; set; }
        DbSet<UserReportCustomization> UserReportCustomizations { get; set; }
        DbSet<StationeryBook> StationaryBooks { get; set; }
        DbSet<TripScheduleConfiguration> TripScheduleConfigurations { get; set; }
        DbSet<StationeryBookLog> StationaryBookLogs { get; set; }
        DbSet<StationeryBookLogArchive> StationeryBookLogArchives { get; set; }
        DbSet<BrandMaster> BrandMasters { get; set; }//
        DbSet<DriverMaster> DriverMasters { get; set; }//
        DbSet<DueMaster> DueMasters { get; set; }
        DbSet<DueTransactionLog> DueTransactionLogs { get; set; }
        DbSet<ExpenseMaster> ExpenseMasters { get; set; }
        DbSet<SalesOrderRequest> SalesOrders { get; set; }
        DbSet<PMMaster> PmMasters { get; set; }//
        DbSet<PMSchedule> PmSchedules { get; set; }//
        DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        DbSet<RouteMaster> RouteMasters { get; set; }
        DbSet<RouteWayPoint> RouteWayPoints { get; set; }
        DbSet<SpareLog> SpareLogs { get; set; }//
        DbSet<StoreBinMaster> StoreBINMasters { get; set; }
        DbSet<SpareInventoryLevel> SpareInventoryLevels { get; set; }
        DbSet<RepairLabourLog> RepairLabourLogs { get; set; }
        DbSet<SpareMaster> SpareMasters { get; set; }
        DbSet<UnitMaster> UnitMasters { get; set; }
        DbSet<UnitConverter> UnitConversions { get; set; }
        DbSet<TripAdvanceLog> TripAdvanceLogs { get; set; }
        DbSet<TripExpenseLog> TripExpenseLogs { get; set; }

        DbSet<TyreCheck> TyreChecks { get; set; }//
        DbSet<TyreLog> TyreLogs { get; set; }//
        DbSet<TyreLifePerformanceLog> TyreLifePerformanceLogs { get; set; }
        DbSet<TyreMillageLog> TyreMillageLogs { get; set; }
        DbSet<TyreMaster> TyreMasters { get; set; }//
        DbSet<TyreRubberType> TyreRubberTypes { get; set; }//
        DbSet<VehicleMaster> VehicleMasters { get; set; }//
        DbSet<HireVehicle> HireVehicles { get; set; }
        DbSet<VehicleModel> VehicleModels { get; set; }//
        DbSet<VehicleMonthlyBudget> VehicleMonthlyBudgets { get; set; }
        DbSet<VehicleMovementLog> VehicleMovementLogs { get; set; }
        DbSet<VehicleTripSettlement> VehicleTripSettlements { get; set; }
        //20151127: end

        //20151127: start Global
        DbSet<AccountGroup> AccountGroupMasters { get; set; }//
        DbSet<CityMaster> CityMasters { get; set; }//
        DbSet<ConstantType> ConstantTypes { get; set; }//
        DbSet<ConstantValue> ConstantValues { get; set; }//
        DbSet<GenericMaster> GenericMasters { get; set; }//
        DbSet<Ledger> Ledgers { get; set; }//
        DbSet<OfficeMaster> OfficeMasters { get; set; }//
        DbSet<Voucher> Vouchers { get; set; }//
        DbSet<HttpRequestPool> HttpRequestPools { get; set; }
        //20151127: end

        DbSet<DriverIncidentLog> DriverIncidentLogs { get; set; }
        DbSet<DriverGuarantor> DriverGuarantors { get; set; }
        DbSet<DriverPayment> DriverPayments { get; set; }
        DbSet<DriverRelative> DriverRelatives { get; set; }
        DbSet<DriverTrainingLog> DriverTrainingLogs { get; set; }
        DbSet<DueInsuranceLog> DueInsuranceLogs { get; set; }
        //DbSet<RouteCityMap> RouteCityMaps { get; set; }
        DbSet<TollMaster> TollMasters { get; set; }
        DbSet<TollRateLog> TollRateLogs { get; set; }
        DbSet<RouteTollMap> RouteTollMaps { get; set; }
        DbSet<MasterAlias> Aliases { get; set; }
        DbSet<SpareFitmentPosition> SpareFitmentPositions { get; set; }
        DbSet<VehicleDueMapping> VehicleDueMappings { get; set; }
        //DbSet<AliasLog> VehicleMasterAlias { get; set; }
        DbSet<VehicleOwnerMapping> VehicleOwnerMappings { get; set; }

        DbSet<VehicleTrailorMapping> VehicleTrailorMappings { get; set; }
        DbSet<VehicleFuelBudget> VehicleFuelBudgets { get; set; }
        DbSet<VehicleBudget> VehicleBudgets { get; set; }
        DbSet<VehicleMovementLogPickupDrop> VehicleMovementLogPickupDrops { get; set; }
        DbSet<VehicleAccessoryLog> VehicleAccessoryLogs { get; set; }
        DbSet<VoucherDetail> VoucherDetails { get; set; }
        DbSet<VoucherDetailReference> VoucherDetailReferences { get; set; }
        DbSet<VoucherType> VoucherTypes { get; set; }
        DbSet<FinancialYear> FinancialYears { get; set; }
        //DbSet<FinancialYearLockLog> FinancialYearLockLogs { get; set; }
        DbSet<FinancialYearLedgerLockLog> FinancialYearLedgerLocks { get; set; }
        DbSet<VoucherAuditLog> VoucherAuditLogs { get; set; }
        DbSet<ApiConfiguration> ApiConfigurations { get; set; }
        DbSet<ClientConfiguration> ClientConfigurations { get; set; }
        DbSet<VoucherTypeGroupMapping> VoucherTypeGroupMappings { get; set; }
        DbSet<ViewField> ViewFields { get; set; }
        DbSet<ViewFieldBookMap> ViewFieldBookMaps { get; set; }
        DbSet<CNMultiMaterial> CnMultiMaterials { get; set; }
        DbSet<CNEWayBill> CNEWayBills { get; set; }
        DbSet<PostalAddress> PostalAddresses { get; set; }
        DbSet<Country> Countries { get; set; }
        DbSet<ObjectCategory> ObjectCategories { get; set; }
        DbSet<ObjectClass> ObjectClasses { get; set; }
        DbSet<ObjectClassMap> ObjectClassesMapping { get; set; }
        DbSet<VehiclePreventiveLog> VehiclePreventiveLogs { get; set; }
        DbSet<VehicleRepairJob> VehicleRepairJobs { get; set; }
        
        DbSet<BatteryBrand> BatteryBrands { get; set; }
        DbSet<BatteryCheck> BatteryChecks { get; set; }
        DbSet<BatteryLifePerformanceLog> BatteryLifePerformanceLogs { get; set; }
        DbSet<BatteryLog> BatteryLogs { get; set; }
        DbSet<BatteryLogExtraInfo> BatteryLogExtraInfos { get; set; }
        DbSet<BatteryMaster> BatteryMasters { get; set; }
        DbSet<DriverNextStatusMapping> DriverNextStatusMappings { get; set; }
        DbSet<DriverVehicleMapping> DriverVehicleMappings { get; set; }
        DbSet<LedgerRole> LedgerRoles { get; set; }
        DbSet<ApiScriptMigration> ScriptMigrations { get; set; }
        DbSet<DTSStatusMapping> DTSStatusMappings { get; set; }
        DbSet<DTSStatus> DTSStatus { get; set; }
        DbSet<VTSStatusLog> VTSStatusLogs { get; set; }
        DbSet<VTSStatusLogsub> VTSStatusLogsubs { get; set; }

        DbSet<PrintFormatMaster> PrintFormatMasters { get; set; }
        DbSet<PrintFormatDataSource> PrintFormatDataSources { get; set; }
        DbSet<LedgerPrintFormat> LedgerPrintFormats { get; set; }

        DbSet<VehicleAccidentClaim> AccidentClaims { get; set; }
        DbSet<VehicleAccidentEstimate> VehicleAccidentEstimates { get; set; }
        DbSet<ApiFile> ApiFiles { get; set; }
        DbSet<FileUploadNature> FileUploadNatures { get; set; }
        DbSet<ORMLog> OrmLogs { get; set; }
        DbSet<FleetGatePass> FleetGatePasses { get; set; }
        DbSet<FuelRateLog> FuelRateLogs { get; set; }


        #region BMS
        DbSet<CNMaster> Consignments { get; set; }
        DbSet<CNBillLogArchive> BillLogArchives { get; set; }
        DbSet<CNExtraInfo> CNExtraInfos { get; set; }
        DbSet<VehicleConfigurationLog> VehicleConfigurationLogs { get; set; }
        DbSet<CNStockLog> CnStockLogs { get; set; }
        DbSet<CnStatusLog> CnStatusLogs { get; set; }
        DbSet<CnChallan> CNChallans { get; set; }
        DbSet<CNBillNature> CnBillNatures { get; set; }
        DbSet<LoadType> LoadTypes { get; set; }
        DbSet<MaterialGroup> MaterialGroups { get; set; }
        DbSet<MaterialMaster> MaterialMasters { get; set; }
        DbSet<SalesLog> SalesLog { get; set; }
        /// <summary>
        /// Gets or sets the material masters.
        /// </summary>
        /// <value>The material masters.</value>
        //DbSet<MaterialParty> MaterialParties { get; set; }
        #endregion
        #region AMS
        DbSet<TaxServiceType> TaxServiceTypes { get; set; }
        DbSet<TaxRateMaster> TaxRateMasters { get; set; }
        #endregion
        
        DbSet<IpUserMapping> IpUserMappings { get; set; }
        DbSet<ApiDevice> ApiDevices { get; set; }
        #region Reporting
        DbSet<ReportRequestPool> ReportsRequestPool { get; set; }
        DbSet<ReportProcedure> ReportProcedures { get; set; }
        DbSet<ReportParameter> ReportParameters { get; set; }
        DbSet<ReportCustomization> ReportCustomizations { get; set; }
        #endregion
        #region UserDefined Reports
        DbSet<UserDefinedReport> UserDefinedReports { get; set; }
        DbSet<UserDefinedReportParameter> UserDefinedReportParameters { get; set; }
        DbSet<UserDefinedReportProcedure> UserDefinedReportProcedures { get; set; }
        #endregion
        DbSet<PaymentDeductionType> PaymentDeductionTypes { get; set; }
        DbSet<MaterialDispatchOrder> MaterialDispatchOrders { get; set; } 
        DbSet<CNDTSStatusLog> CNDTSStatusLogs { get; set; }
        DbSet<CNDTSStatus> CNDTSStatuses { get; set; }

        DbSet<ApiPubSubStore> ApiPubSubStores { get; set; }
        DbSet<GeneralExpenseLog> GeneralExpenseLogs { get; set; }
        DbSet<Loan> Loans { get; set; }
        DbSet<LoanLog> LoanLogs { get; set; }
        DbSet<PurchaseOrderLog> PurchaseOrderLogs { get; set; }
        DbSet<BillSubmission> BillSubmissions { get; set; }
        DbSet<OfficeVehicleMap> OfficeVehicleMaps { get; set; }
        DbSet<PartyRouteTime> PartyRouteTimes { get; set; }
        DbSet<Rule> Rules { get; set; }
        DbSet<InlineQuery> InlineQueries { get; set; }
        DbSet<GPSKmLog> GPSKmLogs { get; set; }
        DbSet<GPSStatusLog> GPSStatusLogs { get; set; }
        DbSet<EventStorage> EventStorage { get; set; }
        DbSet<JsonTransactionLog> JsonTransactionLogs { get; set; }
        DbSet<GpsEndPoint> IntegrationEndPoints { get; set; }
        DbSet<IntrgrationServiceLog> IntrgrationServices { get; set; }
        DbSet<RouteVehicleType> RouteVehicleTypes { get; set; }
        DbSet<CardMaster> Cards { get; set; }
        DbSet<VehicleCardMapping> VehicleCardMappings { get; set; }
        DbSet<GSTConfiguration> GSTConfigurations { get; set; }
        DbSet<HMArrival> HMArrivals { get; set; }
        DbSet<HMArrivalLog> HMArrivalLogs { get; set; }
        DbSet<ServiceUnit> ServiceUnits { get; set; }
        DbSet<ServiceMaster> ServiceMasters { get; set; }
        DbSet<CustomerServiceRequest> CustomerServiceRequests { get; set; }
        DbSet<CustomerServiceRequestLog> customerServiceRequestLogs { get; set; }

        DbSet<CurrencyConversion> CurrencyConversions { get; set; }//
        DbSet<APLConfig> APLConfigs { get; set; }
        DbSet<APLType> APLTypes { get; set; }
        DbSet<APLLog> APLLogs { get; set; }
        DbSet<APLLogAnx> APLLogAnxs { get; set; }
        DbSet<APLLogAnxLevel> APLLogAnxLevels { get; set; }

        DbSet<TransactionSupportLog> TransactionSupportLogs { get; set; }
        DbSet<TPTRequestPool> TPTRequestPools { get; set; }

        DbSet<ZRAStandard> ZRAStandards { get; set; }
        DbSet<ZRAStandardCode> ZRAStandardCodes { get; set; }
        DbSet<ZRAClassificationCode> ZRAClassificationCodes { get; set; }

    }
}
