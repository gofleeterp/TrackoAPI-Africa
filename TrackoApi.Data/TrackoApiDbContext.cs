using Repository.Pattern.Ef6;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using Tenant;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Data.Mapping;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Base.Attributes;
using TrackoApi.Models.BMS;
using TrackoApi.Models.CRM;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.FMS.Loan;
using TrackoApi.Models.FMS.Repairs;
using TrackoApi.Models.FMS.Tyres;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Models.Global.DTS;
using TrackoAPI.Reporting.Models;
using Unity;
using Configuration = TrackoApi.Data.Migrations.Configuration;

namespace TrackoApi.Data
{
    public class TrackoApiDbContext : DataContext, ITrackoApiDbContext// IdentityDbContext<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim>, ITrackoApiDbContext
    {
        [InjectionConstructor]
        //#if DEBUG
        //        public TrackoApiDbContext() :base("SafeX") /*base("Data Source=server;Database=SafeXPlus;Integrated Security=False;User ID=sa;Password=123;Connect Timeout=150;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;")*/
        //        {
        //            Init();
        //        }
        //#else
        public TrackoApiDbContext(IGlobalStore globalStore) : base(globalStore,ConnectionHelper.GetConnection(globalStore))
        {
            Init();
        }

        //#endif
        public TrackoApiDbContext(TenantConnection connection, IGlobalStore globalStore) : base(globalStore,string.IsNullOrWhiteSpace(connection.ConnectionString) ? Tenant.ConnectionHelper.GetConnectionByTenentId(globalStore, connection.TenantId) : connection.ConnectionString)
        {
            Init();
        }

        public TrackoApiDbContext(IGlobalStore globalStore,string connection) : base(globalStore,connection)
        {
            Init();
        }

        private void Init()
        {
            var allowMigration = ConfigurationManager.AppSettings["AllowMigration"];
            bool enableMigration = false;
            if (allowMigration != null && bool.TryParse(allowMigration, out enableMigration) && enableMigration)
            {
                //dm.Configuration.CodeGenerator.
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<TrackoApiDbContext, Configuration>(true));
            }

            //Database.Initialize(true);
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;

            base.RequireUniqueEmail = false;
        }

        #region Entities

        public T Attach<T>(T value) where T : class
        {
            return Set<T>().Attach(value);
        }
        public DbSet<GeneralTransaction> GeneralTransactions { get; set; }
        public DbSet<GeneralTransLog> GeneralTransLogs { get; set; }
        public DbSet<UserReportCustomization> UserReportCustomizations { get; set; }
        public DbSet<TripScheduleConfiguration> TripScheduleConfigurations { get; set; }
        public DbSet<Contact> ContactBooks { get; set; }
        public DbSet<JobLog> JobLogs { get; set; }
        public DbSet<HttpRequestPool> HttpRequestPools { get; set; }
        public DbSet<ScheduleLog> ScheduleLogs { get; set; }
        public DbSet<JobRetryLog> JobRetryLogs { get; set; }
        public DbSet<JobScheduleLimit> JobScheduleLimits { get; set; }
        public DbSet<MessageAddress> MessageAddresses { get; set; }
        public DbSet<TemplateMaster> TemplateMasters { get; set; }

        public DbSet<StationeryBook> StationaryBooks { get; set; }
        public DbSet<StationeryBookLog> StationaryBookLogs { get; set; }
        public DbSet<StationeryBookLogArchive> StationeryBookLogArchives { get; set; }
        public DbSet<BrandMaster> BrandMasters { get; set; }
        public DbSet<DriverMaster> DriverMasters { get; set; }
        public DbSet<DueMaster> DueMasters { get; set; }
        public DbSet<DueTransactionLog> DueTransactionLogs { get; set; }
        public DbSet<ExpenseMaster> ExpenseMasters { get; set; }

        public DbSet<PMMaster> PmMasters { get; set; }
        public DbSet<PMSchedule> PmSchedules { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<RouteMaster> RouteMasters { get; set; }
        public DbSet<RouteWayPoint> RouteWayPoints { get; set; }
        public DbSet<SpareLog> SpareLogs { get; set; }
        public DbSet<StoreBinMaster> StoreBINMasters { get; set; }

        public DbSet<SpareInventoryLevel> SpareInventoryLevels { get; set; }

        public DbSet<RepairLabourLog> RepairLabourLogs { get; set; }

        public DbSet<SpareLogExtraInfo> SpareLogsExtraInfo { get; set; }

        public DbSet<SpareMaster> SpareMasters { get; set; }
        public DbSet<UnitMaster> UnitMasters { get; set; }
        public DbSet<UnitConverter> UnitConversions { get; set; }
        public DbSet<TripAdvanceLog> TripAdvanceLogs { get; set; }
        public DbSet<TripExpenseLog> TripExpenseLogs { get; set; }

        public DbSet<TyreCheck> TyreChecks { get; set; }
        public DbSet<TyreLog> TyreLogs { get; set; }
        public DbSet<TyreLifePerformanceLog> TyreLifePerformanceLogs { get; set; }
        public DbSet<TyreMillageLog> TyreMillageLogs { get; set; }
        public DbSet<TyreMaster> TyreMasters { get; set; }
        public DbSet<TyreRubberType> TyreRubberTypes { get; set; }
        public DbSet<VehicleMaster> VehicleMasters { get; set; }
        public DbSet<HireVehicle> HireVehicles { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<VehicleMonthlyBudget> VehicleMonthlyBudgets { get; set; }
        public DbSet<VehicleMovementLog> VehicleMovementLogs { get; set; }
        public DbSet<VehicleTripSettlement> VehicleTripSettlements { get; set; }
        //20151127: end

        //20151127: start Global
        public DbSet<AccountGroup> AccountGroupMasters { get; set; }

        public DbSet<CityMaster> CityMasters { get; set; }
        public DbSet<ConstantType> ConstantTypes { get; set; }
        public DbSet<ConstantValue> ConstantValues { get; set; }
        public DbSet<GenericMaster> GenericMasters { get; set; }
        public DbSet<Ledger> Ledgers { get; set; }
        public DbSet<OfficeMaster> OfficeMasters { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }

        //20151127: end
        public DbSet<DriverIncidentLog> DriverIncidentLogs { get; set; }

        public DbSet<DriverGuarantor> DriverGuarantors { get; set; }
        public DbSet<DriverPayment> DriverPayments { get; set; }
        public DbSet<DriverRelative> DriverRelatives { get; set; }
        public DbSet<DriverTrainingLog> DriverTrainingLogs { get; set; }
        public DbSet<DueInsuranceLog> DueInsuranceLogs { get; set; }

        //public DbSet<RouteCityMap> RouteCityMaps { get; set; }
        public DbSet<TollMaster> TollMasters { get; set; }

        public DbSet<TollRateLog> TollRateLogs { get; set; }
        public DbSet<RouteTollMap> RouteTollMaps { get; set; }
        public DbSet<MasterAlias> Aliases { get; set; }
        public DbSet<SpareFitmentPosition> SpareFitmentPositions { get; set; }
        public DbSet<VehicleDueMapping> VehicleDueMappings { get; set; }

        //public DbSet<AliasLog> VehicleMasterAlias { get; set; }
        public DbSet<VehicleOwnerMapping> VehicleOwnerMappings { get; set; }

        public DbSet<VehicleTrailorMapping> VehicleTrailorMappings { get; set; }
        public DbSet<VehicleFuelBudget> VehicleFuelBudgets { get; set; }
        public DbSet<VehicleBudget> VehicleBudgets { get; set; }
        public DbSet<VehicleMovementLogPickupDrop> VehicleMovementLogPickupDrops { get; set; }
        public DbSet<VehicleAccessoryLog> VehicleAccessoryLogs { get; set; }
        public DbSet<CnStatusLog> CnStatusLogs { get; set; }
        public DbSet<CnChallan> CNChallans { get; set; }
        public DbSet<CNBillNature> CnBillNatures { get; set; }
        public DbSet<LoadType> LoadTypes { get; set; }
        public DbSet<MaterialGroup> MaterialGroups { get; set; }
        public DbSet<MaterialMaster> MaterialMasters { get; set; }

        //public DbSet<MaterialParty> MaterialParties { get; set; }
        public DbSet<TaxServiceType> TaxServiceTypes { get; set; }

        public DbSet<TaxRateMaster> TaxRateMasters { get; set; }
        public DbSet<ReportRequestPool> ReportsRequestPool { get; set; }
        public DbSet<ReportProcedure> ReportProcedures { get; set; }
        public DbSet<VoucherDetail> VoucherDetails { get; set; }
        public DbSet<VoucherDetailReference> VoucherDetailReferences { get; set; }
        public DbSet<VoucherType> VoucherTypes { get; set; }
        //public DbSet<FinancialYearLockLog> FinancialYearLockLogs { get; set; }
        public DbSet<FinancialYearLedgerLockLog> FinancialYearLedgerLocks { get; set; }
        public DbSet<VoucherAuditLog> VoucherAuditLogs { get; set; }

        public DbSet<FuelRateLog> FuelRateLogs { get; set; }
        public DbSet<CNMaster> Consignments { get; set; }
        public DbSet<CNBillLogArchive> BillLogArchives { get; set; }
        public DbSet<CNExtraInfo> CNExtraInfos { get; set; }
        public DbSet<VehicleConfigurationLog> VehicleConfigurationLogs { get; set; }
        public DbSet<CNStockLog> CnStockLogs { get; set; }

        public DbSet<ClientConfiguration> ClientConfigurations { get; set; }
        public DbSet<VoucherTypeGroupMapping> VoucherTypeGroupMappings { get; set; }
        public DbSet<ViewField> ViewFields { get; set; }
        public DbSet<ViewFieldBookMap> ViewFieldBookMaps { get; set; }
        public DbSet<CNMultiMaterial> CnMultiMaterials { get; set; }
        public DbSet<CNEWayBill> CNEWayBills { get; set; }
        public DbSet<EWBUpdateLog> EWBUpdateLogs { get; set; }
        public DbSet<PostalAddress> PostalAddresses { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<ObjectCategory> ObjectCategories { get; set; }
        public DbSet<ObjectClass> ObjectClasses { get; set; }
        public DbSet<ObjectClassMap> ObjectClassesMapping { get; set; }
        public DbSet<VehiclePreventiveLog> VehiclePreventiveLogs { get; set; }
        public DbSet<VehicleRepairJob> VehicleRepairJobs { get; set; }
        public DbSet<ReportParameter> ReportParameters { get; set; }

        public DbSet<ReportCustomization> ReportCustomizations { get; set; }
        public DbSet<BatteryBrand> BatteryBrands { get; set; }
        public DbSet<BatteryCheck> BatteryChecks { get; set; }
        public DbSet<BatteryLifePerformanceLog> BatteryLifePerformanceLogs { get; set; }
        public DbSet<BatteryLog> BatteryLogs { get; set; }
        public DbSet<BatteryLogExtraInfo> BatteryLogExtraInfos { get; set; }
        public DbSet<BatteryMaster> BatteryMasters { get; set; }
        public DbSet<DriverNextStatusMapping> DriverNextStatusMappings { get; set; }
        public DbSet<DriverVehicleMapping> DriverVehicleMappings { get; set; }
        public DbSet<LedgerRole> LedgerRoles { get; set; }
        public DbSet<ApiScriptMigration> ScriptMigrations { get; set; }
        public DbSet<DTSStatusMapping> DTSStatusMappings { get; set; }
        public DbSet<DTSStatus> DTSStatus { get; set; }
        public DbSet<VTSStatusLog> VTSStatusLogs { get; set; }
        public DbSet<VTSStatusLogsub> VTSStatusLogsubs { get; set; }

        public DbSet<PrintFormatMaster> PrintFormatMasters { get; set; }
        public DbSet<PrintFormatDataSource> PrintFormatDataSources { get; set; }
        public DbSet<LedgerPrintFormat> LedgerPrintFormats { get; set; }
        public DbSet<VehicleAccidentClaim> AccidentClaims { get; set; }
        public DbSet<VehicleAccidentEstimate> VehicleAccidentEstimates { get; set; }
        public DbSet<ApiFile> ApiFiles { get; set; }
        public DbSet<FileUploadNature> FileUploadNatures { get; set; }
        public DbSet<ORMLog> OrmLogs { get; set; }
        public DbSet<FleetGatePass> FleetGatePasses { get; set; }
        public DbSet<SalesLog> SalesLog { get; set; }
        public DbSet<GPSKmLog> GPSKmLogs { get; set; }
        public DbSet<GPSStatusLog> GPSStatusLogs { get; set; }
        public DbSet<EventStorage> EventStorage { get; set; }
        public DbSet<JsonTransactionLog> JsonTransactionLogs { get; set; }
        public DbSet<RouteVehicleType> RouteVehicleTypes { get; set; }
        public DbSet<GSTConfiguration> GSTConfigurations { get; set; }
        public DbSet<CardMaster> Cards { get; set; }
        public DbSet<VehicleCardMapping> VehicleCardMappings { get; set; }
        public DbSet<IpUserMapping> IpUserMappings { get; set; }
        public DbSet<ApiDevice> ApiDevices { get; set; }
        public DbSet<ServiceUnit> ServiceUnits { get; set; }
        public DbSet<ServiceMaster> ServiceMasters { get; set; }
        public DbSet<CustomerServiceRequest> CustomerServiceRequests { get; set; }
        public DbSet<CustomerServiceRequestLog> customerServiceRequestLogs { get; set; }

        public DbSet<CurrencyConversion> CurrencyConversions { get; set; }
        public DbSet<APLConfig> APLConfigs { get; set; }
        public DbSet<APLType> APLTypes { get; set; }
        public DbSet<APLLog> APLLogs { get; set; }
        public DbSet<APLLogAnx> APLLogAnxs { get; set; }
        public DbSet<APLLogAnxLevel> APLLogAnxLevels { get; set; }
        public DbSet<TransactionSupportLog> TransactionSupportLogs { get; set; }
        public DbSet<TPTRequestPool> TPTRequestPools { get; set; }

        public DbSet<ZRAStandard> ZRAStandards { get; set; }
        public DbSet<ZRAStandardCode> ZRAStandardCodes { get; set; }
        public DbSet<ZRAClassificationCode> ZRAClassificationCodes { get; set; }

        #endregion Entities



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<SqlDefaultValueAttribute, string>("SqlDefaultValue", (p, attributes) => attributes.Single().DefaultValue));
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.AddFromAssembly(Assembly.GetAssembly(typeof(ConfigureDbMappings)));
        }

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword

        public IDbSet<T> Set<T>() where T : class => base.Set<T>();

#pragma warning restore CS0114 // Member hides inherited member; missing override keyword

        #region UserDefined Reports

        public DbSet<UserDefinedReport> UserDefinedReports { get; set; }
        public DbSet<UserDefinedReportParameter> UserDefinedReportParameters { get; set; }
        public DbSet<UserDefinedReportProcedure> UserDefinedReportProcedures { get; set; }
        public DbSet<PaymentDeductionType> PaymentDeductionTypes { get; set; }
        public DbSet<MaterialDispatchOrder> MaterialDispatchOrders { get; set; }
        public DbSet<CNDTSStatusLog> CNDTSStatusLogs { get; set; }
        public DbSet<CNDTSStatus> CNDTSStatuses { get; set; }
        public DbSet<ApiPubSubStore> ApiPubSubStores { get; set; }
        public DbSet<GeneralExpenseLog> GeneralExpenseLogs { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanLog> LoanLogs { get; set; }
        public DbSet<PurchaseOrderLog> PurchaseOrderLogs { get; set; }
        public DbSet<BillSubmission> BillSubmissions { get; set; }
        public DbSet<OfficeVehicleMap> OfficeVehicleMaps { get; set; }
        public DbSet<PartyRouteTime> PartyRouteTimes { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<InlineQuery> InlineQueries { get; set; }
        public DbSet<GpsEndPoint> IntegrationEndPoints { get; set; }
        public DbSet<IntrgrationServiceLog> IntrgrationServices { get; set; }
        public DbSet<HMArrival> HMArrivals { get; set; }
        public DbSet<HMArrivalLog> HMArrivalLogs { get; set; }
        public DbSet<SalesOrderRequest> SalesOrders { get; set; }

        #endregion UserDefined Reports
    }
}