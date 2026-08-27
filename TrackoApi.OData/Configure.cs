using CronExpressionDescriptor;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.OData.Builder;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.CRM;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.FMS.Loan;
using TrackoApi.Models.FMS.Repairs;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Models.Global.DTS;
using TrackoApi.OData.ReportFunctions;
using TrackoAPI.Models.Shared;
using TrackoAPI.Reporting.Models;
using TrackoAPI.ViewModels;
using TrackoAPI.ViewModels.AMS;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.ViewModels.FMS.Battery;
using TrackoAPI.ViewModels.FMS.Dues;
using TrackoAPI.ViewModels.FMS.Repairs;
using TrackoAPI.ViewModels.FMS.Tyres;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;

namespace TrackoApi.OData
{
    public class Configure
    {
        //public static Microsoft.OData.Edm.IEdmModel GetEdmModel()
        public static ODataConventionModelBuilder GetEdmModelBuilder(Action<ODataConventionModelBuilder> modelBuilder = null)
        {

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder
            {
                Namespace = "trackoApi",
                ContainerName = "TrackoApiContext",
                DataServiceVersion = new Version(4, 0)

            };
            #region Api Infrastructure
            builder.AddComplexType(typeof(RegisterUser));
            builder.AddComplexType(typeof(vwRole));
            builder.AddComplexType(typeof(vwUserName));
            builder.AddComplexType(typeof(vwApiDevice));
            builder.AddComplexType(typeof (vwApiRolePermission));
            builder.AddComplexType(typeof(CronViewModel));
            builder.EntityType<ApiUser>();
            var aur=builder.EntityType<ApiUserRole>();
            aur.HasKey(x=>new {x.UserId,x.RoleId });

            //ChangePassword
            builder.AddComplexType(typeof(ChangePassword));
            //var role=builder.EntitySet<ApiRolePermission>("RolePermissions");
            builder.AddComplexType(typeof(vwCNMultiMaterial));
            builder.AddComplexType(typeof(vwBillPaymentLog));
            builder.AddComplexType(typeof(vwSparePurchaseBill));
            builder.AddComplexType(typeof(vwTripAdvanceLog));
            builder.AddComplexType(typeof(vwTSL));

            builder.AddComplexType(typeof(TripFuelExpense));
            builder.AddComplexType(typeof(vwGeneralExpenseLog));
            builder.AddComplexType(typeof(vwTyreBillView));
            builder.AddComplexType(typeof(vwTyreChassisBill));
            builder.AddComplexType(typeof(vwChallanCN));
            builder.AddComplexType(typeof(vwCnChallanCharges));
            builder.AddComplexType(typeof(vwCNStockMMLog));
            builder.AddComplexType(typeof(TrackoAPI.vw.ts.Expense));
            builder.AddComplexType(typeof(TrackoAPI.vw.ts.TripLog));
            builder.AddComplexType(typeof(TrackoAPI.vw.ts.Advance));

            builder.AddComplexType(typeof(FakeVDRs));
            

            builder.AddComplexType(typeof(TrackoAPI.vw.ts.FuelExpense));
            builder.AddComplexType(typeof(vwBatteryBillView));
            builder.AddComplexType(typeof(vwBatteryChassisBill));
            builder.AddComplexType(typeof(FileUploadResult));
            //builder.AddComplexType(typeof(VDRBalance));
            //builder.AddComplexType(typeof(NotificationLogViewModel));
            //builder.AddComplexType(typeof(NotificationPurchaseViewModel));
            //builder.AddComplexType(typeof(EmailResponse));
            //builder.AddComplexType(typeof(SendGridEmailViewModel));
            //builder.AddComplexType(typeof(SMSViewModel));
            //builder.AddComplexType(typeof(SMSResult));
            #endregion
            builder.EntitySet<ApiPubSubStore>("PubSubStore");
            builder.EntitySet<HttpRequestPool>("HttpRequestPools");
            builder.EntitySet<BatchVerification>("BatchVerifications");
            builder.EntitySet<BrandMaster>("BrandMasters");
            builder.EntitySet<DriverIncidentLog>("DriverIncidentLogs");
            builder.EntitySet<DriverGuarantor>("DriverGuarantors");
            builder.EntitySet<DriverMaster>("Drivers");
            builder.EntitySet<DriverPayment>("DriverPayments");
            builder.EntitySet<DriverRelative>("DriverRelatives");
            builder.EntitySet<DriverTrainingLog>("DriverTrainingLogs");
            builder.EntitySet<DueInsuranceLog>("DueInsuranceLogs");
            builder.EntitySet<DueMaster>("DueTypes");
            builder.EntitySet<DueTransactionLog>("DueTransactionLogs");
            builder.EntitySet<vwDueVoucher>("BulkDueLogs");
            builder.EntitySet<ExpenseMaster>("ExpenseMasters");
            builder.EntitySet<BillSubmission>("BillSubmissions");

            builder.EntitySet<MaterialMaster>("MaterialMasters");
            builder.EntitySet<MaterialLocationMap>("MaterialLocationMapping");
            builder.EntitySet<CNDTSStatus>("CNDTSStatuses");
            builder.EntitySet<CNDTSStatusLog>("CNDTSStatusLogs");

            builder.EntitySet<PMMaster>("PMMasters");
            builder.EntitySet<PMSchedule>("PMSchedules");
            builder.EntitySet<VehiclePreventiveLog>("VehiclePMLogs");
            builder.EntitySet<PurchaseOrder>("PurchaseOrders");

            builder.EntitySet<PurchaseRequisitionLog>("PurchaseRequisitionLogs");
            builder.EntitySet<PurchaseRequisition>("PurchaseRequisitions");
            //builder.EntitySet<RouteCityMap>("RouteCities");
            builder.EntitySet<RouteMaster>("RouteMasters");
            builder.EntitySet<RouteWayPoint>("RouteWayPoints");
            builder.EntitySet<RouteTollMap>("RouteTolls");
            var aliases=builder.EntitySet<MasterAlias>("MasterAliases");
            builder.EntitySet<SpareFitmentPosition>("SpareFitmentsPosition");
            builder.EntitySet<SpareLog>("SpareLogs");
            builder.EntitySet<SpareLogExtraInfo>("SpareLogExtraInfos");
            builder.EntitySet<RepairLabourLog>("RepairLabourLogs");
            builder.EntitySet<VehicleRepairJob>("VehicleRepairJobs");

            builder.EntitySet<CustomerServiceRequest>("CustomerServiceRequests");
            builder.EntitySet<SpareMaster>("SpareMasters");
            builder.EntitySet<TollMaster>("Tolls");
            builder.EntitySet<TollRateLog>("TollRates");
            builder.EntitySet<vwAdvanceVoucher>("BulkAdvanceInsert");
            
            builder.EntitySet<TripAdvanceLog>("TripAdvanceLogs");
            builder.EntitySet<vwGeneralExpenseVoucher>("BulkGeneralExpenseInsert");
            builder.EntitySet<GeneralExpenseLog>("GeneralExpenseLogs");

            builder.EntitySet<TripExpenseLog>("TripExpenses");
            builder.EntitySet<TyreCheck>("TyreCheckings");
            builder.EntitySet<TyreLog>("TyreLogs");
            builder.EntitySet<TyreLifePerformanceLog>("TyreLifePerformanceLogs");

            builder.EntitySet<JsonTransactionLog>("JsonTransactionLogs");
            builder.EntitySet<TyreMaster>("Tyres");
            builder.EntitySet<TyreLogExtraInfo>("TyreLogExtraInfos");
            builder.EntitySet<TyreRubberType>("TyreRubberTypes");
            builder.EntityType<UnitConverter>();
            builder.EntitySet<UnitMaster>("UnitMasters");
            builder.EntityType<SpareUnitMapping>();
            builder.EntitySet<VehicleBudget>("VehicleBudgets");
            builder.EntitySet<VehicleClass>("VehicleClasses");
            builder.EntitySet<VehicleDueMapping>("VehicleDueMappings");
            builder.EntitySet<VehicleFuelBudget>("VehicleFuelBudgets");
            builder.EntitySet<VehicleMaster>("Vehicles");
            //builder.EntitySet<AliasLog>("VehicleMasterAliases");
            builder.EntitySet<VehicleModel>("VehicleModels");
            builder.EntitySet<VehicleMonthlyBudget>("VehicleMonthlyBudgets");
            
            builder.EntitySet<CNMaster>("Consignments");

            builder.EntitySet<VehicleMovementLog>("VehicleMovementLogs");
            builder.EntitySet<ChallanMaster>("Challans");
            builder.EntitySet<CnChallan>("CNChallans");
            

            builder.EntitySet<VehicleAccessoryLog>("VehicleAccessoryLogs");
            builder.EntitySet<VehicleMovementLogPickupDrop>("VehicleMovementLogPickupDrops");
            builder.EntitySet<VehicleOwnerMapping>("VehileOwnerMappings");
            builder.EntitySet<VehicleTrailorMapping>("TrailorMappings");

            builder.EntitySet<VehicleTripSettlement>("VehicleTripSettlements");
            

            builder.EntitySet<AccountGroup>("AccountGroups");
            builder.EntitySet<CityMaster>("Cities");            
            builder.EntitySet<ConstantType>("ConstantTypes");
            builder.EntitySet<ConstantValue>("ConstantValues");

            builder.EntitySet<ApiView>("Views");
            builder.EntitySet<ApiViewModule>("ViewModules");
            builder.EntitySet<ApiWorkFlowScript>("WorkFlowScripts");
            builder.EntitySet<GenericMaster>("GenericMasters");
            builder.EntitySet<Ledger>("Ledgers");            
            builder.EntitySet<LedgerOffice>("LedgerOffices");
            builder.EntitySet<FinancialYear>("FinancialYears");
            builder.EntitySet<FinancialYearLedgerLockLog>("LedgerLocks");

            builder.EntitySet<PostalAddress>("PostalAddressees");
            builder.EnumType<MasterStatus>();
            builder.EnumType<AclType>();
            builder.EnumType<DrCr>();
            builder.EnumType<ReportParameterType>();
            builder.EnumType<FinanceStatus>();
            builder.EntitySet<OfficeMaster>("Offices");
            builder.EntitySet<Voucher>("Vouchers");
            builder.EntitySet<VoucherDetail>("VoucherDetails");
            builder.EntitySet<VoucherDetailReference>("VoucherDetailReferences");
            builder.EntitySet<VoucherType>("VoucherTypes");
            builder.EntitySet<VoucherTypeGroupMapping>("VoucherTypeGroupMappings");
            builder.EntitySet<ViewField>("ViewFields");
            builder.EntitySet<ViewFieldBookMap>("ViewFieldBookMappings");
            builder.EntitySet<ApiResourceAccessLog>("ResourceAccessLogs");
            builder.EntitySet<ApiRecordAccessLog>("RecordAccessLogs");

            builder.EntitySet<ObjectCategory>("ObjectCategories");
            builder.EntitySet<ObjectClass>("ObjectClasses");
            builder.EntitySet<ObjectClassMap>("ObjectClassMapping");

            builder.EntitySet<ReportParameter>("ReportParameters");
            builder.EntitySet<UserDefinedReportParameter>("UserDefinedReportParameters");
            builder.EntitySet<UserDefinedReport>("UserDefinedReports");
            builder.EntitySet<ReportCustomization>("ReportCustomizations");

            #region Battery Module
            builder.EntitySet<BatteryCheck>("BatteryCheckings");
            builder.EntitySet<BatteryLog>("BatteryLogs");
            builder.EntitySet<BatteryLifePerformanceLog>("BatteryLifePerformanceLogs");
            

            builder.EntitySet<BatteryMaster>("Batteries");
            builder.EntitySet<BatteryLogExtraInfo>("BatteryLogExtraInfos");
            builder.EntitySet<BatteryBrand>("BatteryBrands");
            #endregion

            builder.EntitySet<DriverVehicleMapping>("DriverVehicleMappings");
            builder.EntitySet<DriverNextStatusMapping>("DriverNextStatusMappings");

            builder.EntitySet<LedgerRole>("LedgerRoles");

            builder.EntitySet<StationeryBook>("StationeryBooks");
            builder.EntitySet<StationeryBookLog>("StationeryBookLogs");
            builder.EntitySet<StationeryBookLogArchive>("StationeryBookLogArchives");

            builder.EntitySet<DTSStatusMapping>("DTSStatusMappings");
            builder.EntitySet<DTSStatus>("DTSStatus");
            builder.EntitySet<VTSStatusLog>("VTSStatusLogs");
            builder.EntitySet<VTSStatusLogsub>("VTSStatusLogsubs");

            

            builder.EntitySet<PrintFormatMaster>("PrintFormats");
            builder.EntitySet<PrintFormatDataSource>("PrintFormatDataSources");
            builder.EntitySet<LedgerPrintFormat>("LedgerPrintFormats");

            builder.EntitySet<VehicleAccidentClaim>("VehicleAccidentClaims");
            builder.EntitySet<VehicleAccidentEstimate>("VehicleAccidentEstimates");

            builder.EntitySet<FuelRateLog>("FuelRateLogs");
            builder.EntitySet<ApiFile>("Documents");
            builder.EntitySet<FileUploadNature>("FileUploadNatures");
            builder.EntitySet<FleetGatePass>("FleetGatePasses");
            builder.EntitySet<ORMLog>("ORMLogs");
            builder.EntitySet<ORMAuditLog>("ORMAuditLogs");

            builder.EntitySet<CNBillNature>("CNBillNatures");
            builder.EntitySet<LoadType>("LoadTypes");
            builder.EntitySet<MaterialGroup>("MaterialGroups");
            builder.EntitySet<TaxRateMaster>("TaxRateMasters");
            builder.EntitySet<TaxServiceType>("TaxServiceTypes");
            

            builder.EntitySet<ReportRequestPool>("ReportsRequestPool");
            builder.EntitySet<CNBill>("CNBills");
            builder.EntitySet<CNBillLog>("CNBillLogs");
            builder.EntitySet<CNBillLogArchive>("CNBillLogArchives");
            builder.EntitySet<CNRateContractLog>("CNRateContractLogs");
            builder.EntitySet<PartyContractMap>("PartyContracts");
            builder.EntitySet<CNRateContract>("CNRateContracts");
            builder.EntitySet<CNStockLog>("CNStockLogs");
            builder.EntitySet<vw_CNStockLog>("CNStockLogView");
            builder.EntitySet<vw_CNStockMMLog>("CNStockMMLogView");

            builder.EntitySet<CNBillPayment>("CNBillPayments");
            builder.EntitySet<CNBillPaymentLog>("CNBillPaymentLogs");
            builder.EntitySet<PaymentDeductionType>("PaymentDeductionTypes");
            builder.EntitySet<SpareInventoryLevel>("SpareInventoryLevels");
            builder.EntitySet<SpareBinMapping>("SpareBinMappings");
            builder.EntitySet<StoreBinMaster>("StoreBINMasters");
            builder.EntitySet<CNMultiMaterial>("CNMultiMaterials");
            builder.EntitySet<CNEWayBill>("CNEWayBills");

            // builder.EntitySet<MaterialDispatchOrder>("MaterialDispatchOrders");
            builder.EntitySet<CNStockMMLog>("CNStockMMLogs");
            builder.EntitySet<HireVehicle>("HireVehicles");
            builder.EntitySet<HSAdvance>("HSAdvances");
            builder.EntitySet<MaterialDispatchOrder>("MaterialDispatchOrders");
            //builder.EntitySet<VTSVendorService>("VTSVendorServices");
            builder.EntitySet<TripExpenseBudget>("TripExpenseBudgets");
            builder.EntitySet<Contact>("Contacts");
            builder.EntitySet<ScheduleLog>("Schedules");
            builder.EntitySet<JobLog>("JobLogs");
            builder.EntitySet<TemplateMaster>("TemplateMasters");
            builder.EntitySet<MessageAddress>("MessagesAddresses");
            builder.EntitySet<PurchaseOrderLog>("PurchaseOrderLogs");
            builder.EntitySet<Loan>("Loans");
            builder.EntitySet<LoanLog>("LoanLogs");
            builder.EntitySet<OfficeVehicleMap>("OfficeVehicleMaps");
            builder.EntitySet<GpsEndPoint>("GpsEndPoints");
            builder.EntitySet<IntrgrationServiceLog>("IntrgrationServices");
            builder.EntitySet<GPSStatusLog>("GPSStatusLogs");
            
            builder.EntitySet<PartyRouteTime>("PartyRouteTime");
            builder.EntitySet<SalesLog>("SalesLog");
            builder.EntitySet<SalesOrderRequest>("SalesOrderRequests");
            builder.EnumType<DeviceOS>();
            builder.EntitySet<ReportProcedure>("ReportProcs");
            builder.EntitySet<GPSKmLog>("GPSKmLog");
            builder.EntitySet<RouteVehicleType>("RouteVehicleTypes");
            builder.EntitySet<Rule>("Rules");
            builder.EntitySet<CardMaster>("Cards");
            builder.EntitySet<VehicleCardMapping>("VehicleCardMappings");
            builder.EntitySet<CNExtraInfo>("CNExtraInfos");
            builder.EntitySet<VehicleConfigurationLog>("VehicleConfigurationLogs");
            builder.EntitySet<TripScheduleConfiguration>("TripScheduleConfigurations");
            builder.EntitySet<UserReportCustomization>("UserReportCustomizations");

            builder.EntitySet<HMArrival>("HMArrivals");
            builder.EntitySet<HMArrivalLog>("HMArrivalLogs");

            builder.EntitySet<GeneralTransaction>("GTrans");
            builder.EntitySet<GeneralTransLog>("GTranLogs");

            builder.EntitySet<CurrencyConversion>("CurrencyConversions");
            builder.EntitySet<APLConfig>("APLConfigs");
            builder.EntitySet<APLType>("APLTypes");
            builder.EntitySet<APLLog>("APLLogs");
            builder.EntitySet<APLLogAnx>("APLLogAnxs");
            builder.EntitySet<APLLogAnxLevel>("APLLogAnxLevels");
            builder.EntitySet<TransactionSupportLog>("TransactionSupportLogs");
            builder.EntitySet<TPTRequestPool>("TPTRequestPools");

            builder.EntitySet<ZRAStandard>("ZRAStandards");
            builder.EntitySet<ZRAStandardCode>("ZRAStandardCodes");
            builder.EntitySet<ZRAClassificationCode>("ZRAClassificationCodes");

            BuildActions.RegisterFms(builder);
            BuildActions.RegisterAMS(builder);
            BuildFunctions.RegisterFMS(builder);
            FMSReportFunctions.Register(builder);
            var auttoStationaryProp = typeof(AuditableEntity).GetProperty("AutoStationaryFieldId");            
            foreach(var entity in builder.StructuralTypes.Where(t => t.ClrType.BaseType == typeof(AuditableEntity)))
            {
                entity.AddProperty(auttoStationaryProp).IsOptional();
            }

            //var model= builder.GetEdmModel();
            //return model;
            modelBuilder?.Invoke(builder);
            return builder;
        }

        private static string Camelize(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (text.Length == 0)
            {
                return text;
            }

            var stringBuilder = new StringBuilder(text);
            stringBuilder[0] = char.ToLowerInvariant(text[0]);

            return stringBuilder.ToString();
        }
        private static void SetPrivateFieldValue<T>(object obj, string fieldName, T value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            Type type = obj.GetType();
            FieldInfo fieldInfo = null;

            while (fieldInfo == null && type != null)
            {
                fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                type = type.BaseType;
            }

            if (fieldInfo == null)
            {
                throw new ArgumentOutOfRangeException(
                    "fieldName",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Field {0} was not found in Type {1}",
                        fieldName,
                        obj.GetType().FullName));
            }

            fieldInfo.SetValue(obj, value);
        }
    }
}
