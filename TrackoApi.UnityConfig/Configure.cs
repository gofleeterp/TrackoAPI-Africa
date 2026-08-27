
using FluentValidation;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
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
using TrackoApi.Models.Validations.FMS;
using TrackoApi.Queue;
using TrackoApi.Service;
using TrackoApi.Service.BMS;
using TrackoApi.Service.Finance;
using TrackoApi.Service.FMS;
using TrackoApi.Service.FMS.GPS;
using TrackoApi.Service.Global;
using TrackoApi.Service.TMS;
using TrackoAPI.Infrastructure;
using TrackoAPI.Reporting.Models;
using TrackoAPI.ViewModels.BMS;
using Unity;
using Unity.Lifetime;

namespace TrackoApi.Unity
{
    public class Configure
    {
        public Configure(IUnityContainer container)
        {
            container.RegisterType<ITaskQueue,TaskQueue>(new ContainerControlledLifetimeManager());
            
            container.RegisterType<IAuthRepository, AuthRepository>();
            container.RegisterType<ITenantRepository, TenantRepository>();
            container.RegisterType<IRepository<ApiConfiguration>, Repository<ApiConfiguration>>();
            
            container.RegisterType<IRepositoryAsync<BrandMaster>, Repository<BrandMaster>>();
            container.RegisterType<IBrandMasterService, BrandMasterService>();
            container.RegisterType<IRepositoryAsync<DriverIncidentLog>, Repository<DriverIncidentLog>>();
            container.RegisterType<IDriverIncidentLogService, DriverIncidentLogService>();
            container.RegisterType<IRepositoryAsync<DriverGuarantor>, Repository<DriverGuarantor>>();
            container.RegisterType<IDriverGuarantorService, DriverGuarantorService>();
            container.RegisterType<IRepositoryAsync<DriverMaster>, Repository<DriverMaster>>();
            container.RegisterType<IDriverMasterService, DriverMasterService>();
            container.RegisterType<IRepositoryAsync<DriverPayment>, Repository<DriverPayment>>();
            container.RegisterType<IDriverPaymentService, DriverPaymentService>();
            container.RegisterType<IRepositoryAsync<DriverRelative>, Repository<DriverRelative>>();
            container.RegisterType<IDriverRelativeService, DriverRelativeService>();

            container.RegisterType<IRepositoryAsync<DriverTrainingLog>, Repository<DriverTrainingLog>>();
            container.RegisterType<IDriverTrainingLogService, DriverTrainingLogService>();

            container.RegisterType<IRepositoryAsync<DueInsuranceLog>, Repository<DueInsuranceLog>>();
            container.RegisterType<IDueInsuranceLogService, DueInsuranceLogService>();
            

            container.RegisterType<IRepositoryAsync<DueTransactionLog>, Repository<DueTransactionLog>>();
            container.RegisterType<IDueTransactionLogService, DueTransactionLogService>();
            container.RegisterType<IRepositoryAsync<ExpenseMaster>, Repository<ExpenseMaster>>();
            container.RegisterType<IExpenseMasterService, ExpenseMasterService>();

            container.RegisterType<IRepositoryAsync<MaterialMaster>, Repository<MaterialMaster>>();
            container.RegisterType<IMaterialMasterService, MaterialMasterService>();

            container.RegisterType<IRepositoryAsync<ApiViewModule>, Repository<ApiViewModule>>();
            container.RegisterType<IViewModuleService, ViewModuleService>();

            container.RegisterType<IRepositoryAsync<PMMaster>, Repository<PMMaster>>();
            container.RegisterType<IPMMasterService, PMMasterService>();

            container.RegisterType<IRepositoryAsync<PurchaseOrder>, Repository<PurchaseOrder>>();
            container.RegisterType<IPurchaseOrderService, PurchaseOrderService>();
            

            container.RegisterType<IRepositoryAsync<PurchaseRequisition>, Repository<PurchaseRequisition>>();
            container.RegisterType<IRepository<PurchaseRequisition>, Repository<PurchaseRequisition>>();

            container.RegisterType<IPurchaseRequisitionLogService, PurchaseRequisitionLogService>();

            container.RegisterType<IRepositoryAsync<PurchaseRequisitionLog>, Repository<PurchaseRequisitionLog>>();
            container.RegisterType<IRepository<PurchaseRequisitionLog>, Repository<PurchaseRequisitionLog>>();
            
            container.RegisterType<IPurchaseRequisitionService, PurchaseRequisitionService>();

            //container.RegisterType<IRepositoryAsync<RouteCityMap>, Repository<RouteCityMap>>();
            //container.RegisterType<IRouteCityMapService, RouteCityMapService>();

            container.RegisterType<IRepositoryAsync<RouteMaster>, Repository<RouteMaster>>();
            container.RegisterType<IRouteMasterService, RouteMasterService>();

            container.RegisterType<IRepositoryAsync<ChildParentRoute>, Repository<ChildParentRoute>>();
            container.RegisterType<IRepository<ChildParentRoute>, Repository<ChildParentRoute>>();

            container.RegisterType<IRepositoryAsync<RouteWayPoint>, Repository<RouteWayPoint>>();
            container.RegisterType<IRepository<RouteWayPoint>, Repository<RouteWayPoint>>();
            container.RegisterType<IRouteWayPointService, RouteWayPointService>();

            container.RegisterType<IRepositoryAsync<RouteTollMap>, Repository<RouteTollMap>>();
            container.RegisterType<IRouteTollMapService, RouteTollMapService>();

            container.RegisterType<IRepositoryAsync<MasterAlias>, Repository<MasterAlias>>();
            container.RegisterType<IMasterAliasService, MasterAliasService>();

            container.RegisterType<IRepositoryAsync<CNDTSStatus>, Repository<CNDTSStatus>>();
            container.RegisterType<ICNDTSStatusService, CNDTSStatusService>();

            container.RegisterType<IRepositoryAsync<CNDTSStatusLog>, Repository<CNDTSStatusLog>>();
            container.RegisterType<ICNDTSStatusLogService, CNDTSStatusLogService>();

            container.RegisterType<IRepositoryAsync<HMArrival>, Repository<HMArrival>>();
            container.RegisterType<IHMArrivalService, HMArrivalService>();

            container.RegisterType<IRepositoryAsync<HMArrivalLog>, Repository<HMArrivalLog>>();
            container.RegisterType<IHMArrivalLogService, HMArrivalLogService>();

            container.RegisterType<IRepositoryAsync<SpareFitmentPosition>, Repository<SpareFitmentPosition>>();
            container.RegisterType<ISpareFitmentPositionService, SpareFitmentPositionService>();
            container.RegisterType<IRepositoryAsync<SpareLog>, Repository<SpareLog>>();

            container.RegisterType<ISpareLogService, SpareLogService>();
            container.RegisterType<IRepositoryAsync<SpareLogExtraInfo>, Repository<SpareLogExtraInfo>>();
            
            container.RegisterType<IRepositoryAsync<SpareMaster>, Repository<SpareMaster>>();
            container.RegisterType<ISpareMasterService, SpareMasterService>();
            container.RegisterType<IRepositoryAsync<TollMaster>, Repository<TollMaster>>();
            container.RegisterType<ITollMasterService, TollMasterService>();
            container.RegisterType<IRepositoryAsync<TollRateLog>, Repository<TollRateLog>>();
            container.RegisterType<ITollRateLogService, TollRateLogService>();
            container.RegisterType<IRepositoryAsync<TripAdvanceLog>, Repository<TripAdvanceLog>>();
            container.RegisterType<ITripAdvanceLogService, TripAdvanceLogService>();
            container.RegisterType<IRepositoryAsync<TripExpenseLog>, Repository<TripExpenseLog>>();
            container.RegisterType<ITripExpenseLogService, TripExpenseLogService>();

            container.RegisterType<IRepositoryAsync<GeneralExpenseLog>, Repository<GeneralExpenseLog>>();
            container.RegisterType<IGeneralExpenseLogService, GeneralExpenseLogService>();

            #region Tyre Configuration
            container.RegisterType<IRepositoryAsync<TyreCheck>, Repository<TyreCheck>>();
            container.RegisterType<IRepository<TyreCheck>, Repository<TyreCheck>>();
            container.RegisterType<ITyreCheckService, TyreCheckService>();
            container.RegisterType<IRepositoryAsync<TyreLog>, Repository<TyreLog>>();
            container.RegisterType<ITyreLogService, TyreLogService>();
            container.RegisterType<IRepositoryAsync<TyreMaster>, Repository<TyreMaster>>();
            container.RegisterType<ITyreMasterService, TyreMasterService>();
            container.RegisterType<IRepositoryAsync<TyreRubberType>, Repository<TyreRubberType>>();
            container.RegisterType<ITyreRubberTypeService, TyreRubberTypeService>();
            container.RegisterType<IRepositoryAsync<TyreLogExtraInfo>, Repository<TyreLogExtraInfo>>();
            container.RegisterType<IRepositoryAsync<TyreLifePerformanceLog>, Repository<TyreLifePerformanceLog>>();
            container.RegisterType<IRepositoryAsync<TyreMillageLog>, Repository<TyreMillageLog>>();
            container.RegisterType<ITyreMillageLogService, TyreMillageLogService>();
            #endregion

            container.RegisterType<IRepositoryAsync<UnitConverter>, Repository<UnitConverter>>();
            container.RegisterType<IUnitConverterService, UnitConverterService>();

            container.RegisterType<IRepositoryAsync<VehicleBudget>, Repository<VehicleBudget>>();
            container.RegisterType<IVehicleBudgetService, VehicleBudgetService>();

            container.RegisterType<IRepositoryAsync<VehicleClass>, Repository<VehicleClass>>();
            container.RegisterType<IVehicleClassService, VehicleClassService>();

            container.RegisterType<IRepositoryAsync<VehicleDueMapping>, Repository<VehicleDueMapping>>();
            container.RegisterType<IVehicleDueMappingService, VehicleDueMappingService>();

            container.RegisterType<IRepositoryAsync<VehicleFuelBudget>, Repository<VehicleFuelBudget>>();
            container.RegisterType<IVehicleFuelBudgetService, VehicleFuelBudgetService>();

            container.RegisterType<IRepositoryAsync<VehicleMaster>, Repository<VehicleMaster>>();
            container.RegisterType<IVehicleMasterService, VehicleMasterService>();

            //container.RegisterType<IRepositoryAsync<AliasLog>, Repository<AliasLog>>();
            container.RegisterType<IRepositoryAsync<VehicleModel>, Repository<VehicleModel>>();
            container.RegisterType<IVehicleModelService, VehicleModelService>();

            container.RegisterType<IRepositoryAsync<VehicleMonthlyBudget>, Repository<VehicleMonthlyBudget>>();
            container.RegisterType<IVehicleMonthlyBudgetService, VehicleMonthlyBudgetService>();

            container.RegisterType<IRepositoryAsync<VehicleMovementLog>, Repository<VehicleMovementLog>>();
            container.RegisterType<IVehicleMovementLogService, VehicleMovementLogService>();

            container.RegisterType<IRepositoryAsync<VehicleAccessoryLog>, Repository<VehicleAccessoryLog>>();
            container.RegisterType<IVehicleAccessoryLogService, VehicleAccessoryLogService>();

            container.RegisterType<IRepositoryAsync<VehicleMovementLogPickupDrop>, Repository<VehicleMovementLogPickupDrop>>();
            container.RegisterType<IVehicleMovementLogPickupDropService, VehicleMovementLogPickupDropService>();

            container.RegisterType<IRepositoryAsync<VehicleOwnerMapping>, Repository<VehicleOwnerMapping>>();
            container.RegisterType<IVehicleOwnerMappingService, VehicleOwnerMappingService>();

            container.RegisterType<IRepositoryAsync<VehicleTrailorMapping>, Repository<VehicleTrailorMapping>>();
            container.RegisterType<IVehicleTrailorMappingService, VehicleTrailorMappingService>();

            container.RegisterType<IRepositoryAsync<VehicleTripSettlement>, Repository<VehicleTripSettlement>>();
            container.RegisterType<IVehicleTripSettlementService, VehicleTripSettlementService>();

            container.RegisterType<IRepositoryAsync<AccountGroup>, Repository<AccountGroup>>();
            container.RegisterType<IAccountGroupService, AccountGroupService>();

            container.RegisterType<IRepositoryAsync<CityMaster>, Repository<CityMaster>>();
            container.RegisterType<ICityMasterService, CityMasterService>();

            container.RegisterType<IRepositoryAsync<ConstantType>, Repository<ConstantType>>();
            container.RegisterType<IConstantTypeService, ConstantTypeService>();

            container.RegisterType<IRepositoryAsync<ConstantValue>, Repository<ConstantValue>>();
            container.RegisterType<IConstantValueService, ConstantValueService>();

            container.RegisterType<IRepositoryAsync<ApiView>, Repository<ApiView>>();

            container.RegisterType<IRepositoryAsync<ApiWorkFlowScript>, Repository<ApiWorkFlowScript>>();
            container.RegisterType<IWorkFlowScriptService, WorkFlowScriptService>();

            container.RegisterType<IRepositoryAsync<GenericMaster>, Repository<GenericMaster>>();
            container.RegisterType<IGenericMasterService, GenericMasterService>();
            container.RegisterType<IRepositoryAsync<Ledger>, Repository<Ledger>>();
            container.RegisterType<ILedgerService, LedgerService>();

            container.RegisterType<IRepositoryAsync<FinancialYear>, Repository<FinancialYear>>();
            container.RegisterType<IRepositoryAsync<FinancialYearLedgerLockLog>, Repository<FinancialYearLedgerLockLog>>();

            //PostalAddress
            container.RegisterType<IRepositoryAsync<PostalAddress>, Repository<PostalAddress>>();
            container.RegisterType<IPostalAddressService, PostalAddressService>();


            container.RegisterType<IRepositoryAsync<OfficeMaster>, Repository<OfficeMaster>>();
            container.RegisterType<IOfficeMasterService, OfficeMasterService>();
            container.RegisterType<IRepositoryAsync<Voucher>, Repository<Voucher>>();
            
            container.RegisterType<IVoucherService, VoucherService>();
            container.RegisterType<IRepositoryAsync<VoucherDetail>, Repository<VoucherDetail>>();
            container.RegisterType<IVoucherDetailService, VoucherDetailService>();
            container.RegisterType<IRepositoryAsync<VoucherDetailReference>, Repository<VoucherDetailReference>>();
            container.RegisterType<IVoucherDetailReferenceService, VoucherDetailReferenceService>();

            container.RegisterType<IUserResourceAccessService, UserResourceAccessService>();
            container.RegisterType<IRepositoryAsync<ApiResourceAccessLog>, Repository<ApiResourceAccessLog>>();

            container.RegisterType<IRecordAccessLogService, RecordAccessLogService>();
            container.RegisterType<IRepositoryAsync<ApiRecordAccessLog>, Repository<ApiRecordAccessLog>>();

            container.RegisterType<IVoucherTypeService, VoucherTypeService>();
            container.RegisterType<IRepositoryAsync<VoucherType>, Repository<VoucherType>>();
            
            container.RegisterType<IVoucherTypeGroupMappingService, VoucherTypeGroupMappingService>();
            container.RegisterType<IRepositoryAsync<VoucherTypeGroupMapping>, Repository<VoucherTypeGroupMapping>>();

            container.RegisterType<IViewFieldService, ViewFieldService>();
            container.RegisterType<IRepositoryAsync<ViewField>, Repository<ViewField>>();
            container.RegisterType<IRepository<ViewField>, Repository<ViewField>>();

            container.RegisterType<IViewFieldBookMapService, ViewFieldBookMapService>();
            container.RegisterType<IRepositoryAsync<ViewFieldBookMap>, Repository<ViewFieldBookMap>>();
            container.RegisterType<IRepository<ViewFieldBookMap>, Repository<ViewFieldBookMap>>();

            container.RegisterType<IConsignmentsService, ConsignmentsService>();
            container.RegisterType<IRepositoryAsync<CNMaster>, Repository<CNMaster>>();

            container.RegisterType<IChallanService, ChallanService>();
            container.RegisterType<IRepositoryAsync<ChallanMaster>, Repository<ChallanMaster>>();

            container.RegisterType<ICNChallanService, CNChallanService>();
            container.RegisterType<IRepositoryAsync<CnChallan>, Repository<CnChallan>>();

            container.RegisterType<IObjectCategoryService, ObjectCategoryService>();
            container.RegisterType<IRepositoryAsync<ObjectCategory>, Repository<ObjectCategory>>();

            container.RegisterType<IObjectClassService, ObjectClassService>();
            container.RegisterType<IRepositoryAsync<ObjectClass>, Repository<ObjectClass>>();

            container.RegisterType<IObjectClassMapService, ObjectClassMapService>();
            container.RegisterType<IRepositoryAsync<ObjectClassMap>, Repository<ObjectClassMap>>();

            container.RegisterType<IPMScheduleService, PMScheduleService>();
            container.RegisterType<IRepositoryAsync<PMSchedule>, Repository<PMSchedule>>();
            container.RegisterType<IRepository<PMSchedule>, Repository<PMSchedule>>();

            container.RegisterType<IVehiclePMService, VehiclePMService>();
            container.RegisterType<IRepositoryAsync<VehiclePreventiveLog>, Repository<VehiclePreventiveLog>>();
            container.RegisterType<IRepository<VehiclePreventiveLog>, Repository<VehiclePreventiveLog>>();

            container.RegisterType<IRepairLabourLogService, RepairLabourLogService>();
            container.RegisterType<IRepositoryAsync<RepairLabourLog>, Repository<RepairLabourLog>>();
            container.RegisterType<IRepository<RepairLabourLog>, Repository<RepairLabourLog>>();

            container.RegisterType<IVehicleRepairJobService, VehicleRepairJobService>();
            container.RegisterType<IRepositoryAsync<VehicleRepairJob>, Repository<VehicleRepairJob>>();
            container.RegisterType<IRepository<VehicleRepairJob>, Repository<VehicleRepairJob>>();

            container.RegisterType<IViewService, ViewService>();
            container.RegisterType<IRepositoryAsync<ApiView>, Repository<ApiView>>();
            container.RegisterType<IRepository<ApiView>, Repository<ApiView>>();

            container.RegisterType<IRolePermissionService, RolePermissionService>();
            container.RegisterType<IRepositoryAsync<ApiRolePermission>, Repository<ApiRolePermission>>();
            container.RegisterType<IRepository<ApiRolePermission>, Repository<ApiRolePermission>>();

            #region Battery Configuration
            container.RegisterType<IRepository<BatteryBrand>, Repository<BatteryBrand>>();
            container.RegisterType<IRepositoryAsync<BatteryBrand>, Repository<BatteryBrand>>();
            container.RegisterType<IBatteryBrandService, BatteryBrandService>();

            container.RegisterType<IRepositoryAsync<BatteryCheck>, Repository<BatteryCheck>>();
            container.RegisterType<IRepository<BatteryCheck>, Repository<BatteryCheck>>();
            container.RegisterType<IBatteryCheckService, BatteryCheckService>();

            container.RegisterType<IRepository<BatteryLog>, Repository<BatteryLog>>();
            container.RegisterType<IRepositoryAsync<BatteryLog>, Repository<BatteryLog>>();
            container.RegisterType<IBatteryLogService, BatteryLogService>();

            container.RegisterType<IRepository<BatteryMaster>, Repository<BatteryMaster>>();
            container.RegisterType<IRepositoryAsync<BatteryMaster>, Repository<BatteryMaster>>();
            container.RegisterType<IBatteryMasterService, BatteryMasterService>();

            container.RegisterType<IRepositoryAsync<BatteryLogExtraInfo>, Repository<BatteryLogExtraInfo>>();
            container.RegisterType<IRepository<BatteryLogExtraInfo>, Repository<BatteryLogExtraInfo>>();

            container.RegisterType<IRepositoryAsync<BatteryLifePerformanceLog>, Repository<BatteryLifePerformanceLog>>();
            container.RegisterType<IRepository<BatteryLifePerformanceLog>, Repository<BatteryLifePerformanceLog>>();
            #endregion

            container.RegisterType<IReportParameterService, ReportParameterService>();//Custom
            container.RegisterType<IRepositoryAsync<ReportParameter>, Repository<ReportParameter>>();
            container.RegisterType<IRepository<ReportParameter>, Repository<ReportParameter>>();

            container.RegisterType<ICustomReportParameterService, CustomReportParameterService>();//Custom
            container.RegisterType<IRepositoryAsync<UserDefinedReportParameter>, Repository<UserDefinedReportParameter>>();
            container.RegisterType<IRepository<UserDefinedReportParameter>, Repository<UserDefinedReportParameter>>();
            container.RegisterType<IRepositoryAsync<UserDefinedReport>, Repository<UserDefinedReport>>();
            container.RegisterType<IRepository<UserDefinedReport>, Repository<UserDefinedReport>>();

            container.RegisterType<IReportCustomizationService, ReportCustomizationService>();
            container.RegisterType<IRepositoryAsync<ReportCustomization>, Repository<ReportCustomization>>();
            container.RegisterType<IRepository<ReportCustomization>, Repository<ReportCustomization>>();

            container.RegisterType<IDriverNextStatusMappingService, DriverNextStatusMappingService>();
            container.RegisterType<IRepositoryAsync<DriverNextStatusMapping>, Repository<DriverNextStatusMapping>>();
            container.RegisterType<IRepository<DriverNextStatusMapping>, Repository<DriverNextStatusMapping>>();

            container.RegisterType<IDriverVehicleMappingService, DriverVehicleMappingService>();
            container.RegisterType<IRepositoryAsync<DriverVehicleMapping>, Repository<DriverVehicleMapping>>();
            container.RegisterType<IRepository<DriverVehicleMapping>, Repository<DriverVehicleMapping>>();

            container.RegisterType<ILedgerRoleService, LedgerRoleService>();
            container.RegisterType<IRepositoryAsync<LedgerRole>, Repository<LedgerRole>>();
            container.RegisterType<IRepository<LedgerRole>, Repository<LedgerRole>>();

            container.RegisterType<IStationeryBookService, StationeryBookService>();
            container.RegisterType<IRepositoryAsync<StationeryBook>, Repository<StationeryBook>>();
            container.RegisterType<IRepository<StationeryBook>, Repository<StationeryBook>>();

            container.RegisterType<IStationeryBookLogService, StationeryBookLogService>();
            container.RegisterType<IRepositoryAsync<StationeryBookLog>, Repository<StationeryBookLog>>();
            container.RegisterType<IRepository<StationeryBookLog>, Repository<StationeryBookLog>>();
            //
            container.RegisterType<IStationeryBookLogArchiveService, StationeryBookLogArchiveService>();
            container.RegisterType<IRepositoryAsync<StationeryBookLogArchive>, Repository<StationeryBookLogArchive>>();
            container.RegisterType<IRepository<StationeryBookLogArchive>, Repository<StationeryBookLogArchive>>();

            container.RegisterType<IDTSStatusMappingService, DTSStatusMappingService>();
            container.RegisterType<IRepositoryAsync<DTSStatusMapping>, Repository<DTSStatusMapping>>();
            container.RegisterType<IRepository<DTSStatusMapping>, Repository<DTSStatusMapping>>();

            container.RegisterType<IDTSStatusService, DTSStatusService>();
            container.RegisterType<IRepositoryAsync<DTSStatus>, Repository<DTSStatus>>();
            container.RegisterType<IRepository<DTSStatus>, Repository<DTSStatus>>();

            container.RegisterType<IVTSStatusLogService, VTSStatusLogService>();
            container.RegisterType<IRepositoryAsync<VTSStatusLog>, Repository<VTSStatusLog>>();
            container.RegisterType<IRepository<VTSStatusLog>, Repository<VTSStatusLog>>();

            container.RegisterType<IRepositoryAsync<VTSStatusLogsub>, Repository<VTSStatusLogsub>>();
            container.RegisterType<IRepository<VTSStatusLogsub>, Repository<VTSStatusLogsub>>();


            container.RegisterType<IPrintFormatService, PrintFormatService>();
            container.RegisterType<IRepositoryAsync<PrintFormatMaster>, Repository<PrintFormatMaster>>();
            container.RegisterType<IRepository<PrintFormatMaster>, Repository<PrintFormatMaster>>();

            container.RegisterType<ILedgerPrintFormatService, LedgerPrintFormatService>();
            container.RegisterType<IRepositoryAsync<LedgerPrintFormat>, Repository<LedgerPrintFormat>>();
            container.RegisterType<IRepository<LedgerPrintFormat>, Repository<LedgerPrintFormat>>();

            container.RegisterType<IRepositoryAsync<PrintFormatDataSource>, Repository<PrintFormatDataSource>>();
            container.RegisterType<IRepository<PrintFormatDataSource>, Repository<PrintFormatDataSource>>();

            container.RegisterType<IVehicleAccidentClaimService, VehicleAccidentClaimService>();
            container.RegisterType<IRepositoryAsync<VehicleAccidentClaim>, Repository<VehicleAccidentClaim>>();
            container.RegisterType<IRepository<VehicleAccidentClaim>, Repository<VehicleAccidentClaim>>();

            container.RegisterType<IVehicleAccidentEstimateService, VehicleAccidentEstimateService>();
            container.RegisterType<IRepositoryAsync<VehicleAccidentEstimate>, Repository<VehicleAccidentEstimate>>();
            container.RegisterType<IRepository<VehicleAccidentEstimate>, Repository<VehicleAccidentEstimate>>();

            container.RegisterType<IFuelRateLogService, FuelRateLogService>();
            container.RegisterType<IRepositoryAsync<FuelRateLog>, Repository<FuelRateLog>>();
            container.RegisterType<IRepository<FuelRateLog>, Repository<FuelRateLog>>();

            container.RegisterType<IRepositoryAsync<ApiFile>, Repository<ApiFile>>();
            container.RegisterType<IRepository<ApiFile>, Repository<ApiFile>>();
            container.RegisterType<IDocumetsService, DocumetsService>();

            container.RegisterType<IRepositoryAsync<FileUploadNature>, Repository<FileUploadNature>>();
            container.RegisterType<IRepository<FileUploadNature>, Repository<FileUploadNature>>();
            container.RegisterType<IFileUploadNatureService, FileUploadNatureService>();

            container.RegisterType<IORMLogService, ORMLogService>();
            container.RegisterType<IRepositoryAsync<ORMLog>, Repository<ORMLog>>();
            container.RegisterType<IRepository<ORMLog>, Repository<ORMLog>>();

            container.RegisterType<IORMAuditLogService, ORMAuditLogService>();
            container.RegisterType<IRepositoryAsync<ORMAuditLog>, Repository<ORMAuditLog>>();
            container.RegisterType<IRepository<ORMAuditLog>, Repository<ORMAuditLog>>();
            
            container.RegisterType<IUnitMasterService, UnitMasterService>();
            container.RegisterType<IRepositoryAsync<UnitMaster>, Repository<UnitMaster>>();
            container.RegisterType<IRepository<UnitMaster>, Repository<UnitMaster>>();

            container.RegisterType<ICNBillNatureService, CNBillNatureService>();
            container.RegisterType<IRepositoryAsync<CNBillNature>, Repository<CNBillNature>>();
            container.RegisterType<IRepository<CNBillNature>, Repository<CNBillNature>>();

            container.RegisterType<ILoadTypeService, LoadTypeService>();
            container.RegisterType<IRepositoryAsync<LoadType>, Repository<LoadType>>();
            container.RegisterType<IRepository<LoadType>, Repository<LoadType>>();

            container.RegisterType<IMaterialGroupService, MaterialGroupService>();
            container.RegisterType<IRepositoryAsync<MaterialGroup>, Repository<MaterialGroup>>();
            container.RegisterType<IRepository<MaterialGroup>, Repository<MaterialGroup>>();

            container.RegisterType<ITaxRateMasterService, TaxRateMasterService>();
            container.RegisterType<IRepositoryAsync<TaxRateMaster>, Repository<TaxRateMaster>>();
            container.RegisterType<IRepository<TaxRateMaster>, Repository<TaxRateMaster>>();
            
            container.RegisterType<IFleetGatePassService, FleetGatePassService>();
            container.RegisterType<IRepositoryAsync<FleetGatePass>, Repository<FleetGatePass>>();
            container.RegisterType<IRepository<FleetGatePass>, Repository<FleetGatePass>>();

            container.RegisterType<ITaxServiceTypeService, TaxServiceTypeService>();
            container.RegisterType<IRepositoryAsync<TaxServiceType>, Repository<TaxServiceType>>();
            container.RegisterType<IRepository<TaxServiceType>, Repository<TaxServiceType>>();

            container.RegisterType<IRepositoryAsync<ReportRequestPool>, Repository<ReportRequestPool>>();
            container.RegisterType<IRepository<ReportRequestPool>, Repository<ReportRequestPool>>();

            container.RegisterType<IRepositoryAsync<ReportProcedure>, Repository<ReportProcedure>>();
            container.RegisterType<IRepository<ReportProcedure>, Repository<ReportProcedure>>();

            container.RegisterType<IRepositoryAsync<UserDefinedReportProcedure>, Repository<UserDefinedReportProcedure>>();
            container.RegisterType<IRepository<UserDefinedReportProcedure>, Repository<UserDefinedReportProcedure>>();

            container.RegisterType<ICNBillService, CNBillService>();
            container.RegisterType<IRepositoryAsync<CNBill>, Repository<CNBill>>();
            container.RegisterType<IRepository<CNBill>, Repository<CNBill>>();

            container.RegisterType<ICNBillLogService, CNBillLogService>();
            container.RegisterType<IRepositoryAsync<CNBillLog>, Repository<CNBillLog>>();
            container.RegisterType<IRepository<CNBillLog>, Repository<CNBillLog>>();

            container.RegisterType<ICNBillLogArchiveService, CNBillLogArchiveService>();
            container.RegisterType<IRepositoryAsync<CNBillLogArchive>, Repository<CNBillLogArchive>>();
            container.RegisterType<IRepository<CNBillLogArchive>, Repository<CNBillLogArchive>>();

            container.RegisterType<ICNRateContractLogService, CNRateContractLogService>();
            container.RegisterType<IRepositoryAsync<CNRateContractLog>, Repository<CNRateContractLog>>();
            container.RegisterType<IRepository<CNRateContractLog>, Repository<CNRateContractLog>>();

            container.RegisterType<ICNRateContractService, CNRateContractService>();
            container.RegisterType<IRepositoryAsync<CNRateContract>, Repository<CNRateContract>>();
            container.RegisterType<IRepository<CNRateContract>, Repository<CNRateContract>>();

            container.RegisterType<ICNStockLogService, CNStockLogService>();
            container.RegisterType<IRepositoryAsync<CNStockLog>, Repository<CNStockLog>>();
            container.RegisterType<IRepository<CNStockLog>, Repository<CNStockLog>>();
            container.RegisterType<IRepository<vw_CNStockLog>, Repository<vw_CNStockLog>>();
            container.RegisterType<IRepository<vw_CNStockMMLog>, Repository<vw_CNStockMMLog>>();

            container.RegisterType<ICNBillPaymentService, CNBillPaymentService>();
            container.RegisterType<IRepositoryAsync<CNBillPayment>, Repository<CNBillPayment>>();
            container.RegisterType<IRepository<CNBillPayment>, Repository<CNBillPayment>>();

            container.RegisterType<ICNBillPaymentLogService, CNBillPaymentLogService>();
            container.RegisterType<IRepositoryAsync<CNBillPaymentLog>, Repository<CNBillPaymentLog>>();
            container.RegisterType<IRepository<CNBillPaymentLog>, Repository<CNBillPaymentLog>>();

            container.RegisterType<IRepositoryAsync<PaymentDeductionType>, Repository<PaymentDeductionType>>();
            container.RegisterType<IRepository<PaymentDeductionType>, Repository<PaymentDeductionType>>();

            container.RegisterType<ISpareInventoryLevelService, SpareInventoryLevelService>();
            container.RegisterType<IRepositoryAsync<SpareInventoryLevel>, Repository<SpareInventoryLevel>>();
            container.RegisterType<IRepository<SpareInventoryLevel>, Repository<SpareInventoryLevel>>();

            container.RegisterType<ISpareBinMappingService, SpareBinMappingService>();
            container.RegisterType<IRepositoryAsync<SpareBinMapping>, Repository<SpareBinMapping>>();
            container.RegisterType<IRepository<SpareBinMapping>, Repository<SpareBinMapping>>();

            container.RegisterType<IStoreBINMasterService, StoreBINMasterService>();
            container.RegisterType<IRepositoryAsync<StoreBinMaster>, Repository<StoreBinMaster>>();
            container.RegisterType<IRepository<StoreBinMaster>, Repository<StoreBinMaster>>();

            container.RegisterType<ICNMultiMaterialService, CNMultiMaterialService>();
            container.RegisterType<IRepositoryAsync<CNMultiMaterial>, Repository<CNMultiMaterial>>();
            container.RegisterType<IRepository<CNMultiMaterial>, Repository<CNMultiMaterial>>();

            container.RegisterType<IRepositoryAsync<CNEWayBill>, Repository<CNEWayBill>>();
            container.RegisterType<IRepository<CNEWayBill>, Repository<CNEWayBill>>();

            container.RegisterType<ICNStockMMLogService, CNStockMMLogService>();
            container.RegisterType<IRepositoryAsync<CNStockMMLog>, Repository<CNStockMMLog>>();
            container.RegisterType<IRepository<CNStockMMLog>, Repository<CNStockMMLog>>();

            //  container.RegisterType<IMaterialDispatchOrderService, MaterialDispatchOrderService>();
            //  container.RegisterType<IRepositoryAsync<MaterialDispatchOrder>, Repository<MaterialDispatchOrder>>();
            //  container.RegisterType<IRepository<MaterialDispatchOrder>, Repository<MaterialDispatchOrder>>();

            container.RegisterType<IHireVehicleService, HireVehicleService>();
            container.RegisterType<IRepositoryAsync<HireVehicle>, Repository<HireVehicle>>();
            container.RegisterType<IRepository<HireVehicle>, Repository<HireVehicle>>();

            container.RegisterType<IHSAdvanceService, HSAdvanceService>();
            container.RegisterType<IRepositoryAsync<HSAdvance>, Repository<HSAdvance>>();
            container.RegisterType<IRepository<HSAdvance>, Repository<HSAdvance>>();

            container.RegisterType<IMaterialDispatchOrderService, MaterialDispatchOrderService>();
            container.RegisterType<IRepositoryAsync<MaterialDispatchOrder>, Repository<MaterialDispatchOrder>>();
            container.RegisterType<IRepository<MaterialDispatchOrder>, Repository<MaterialDispatchOrder>>();

            
            container.RegisterType<IRepository<VehicleMonthlyBudget>, Repository<VehicleMonthlyBudget>>();

            //container.RegisterType<IVTSVendorServiceService, VTSVendorServiceService>();
            //container.RegisterType<IRepositoryAsync<VTSVendorService>, Repository<VTSVendorService>>();
            //container.RegisterType<IRepository<VTSVendorService>, Repository<VTSVendorService>>();


            container.RegisterType<ITripExpenseBudgetService, TripExpenseBudgetService>();
            container.RegisterType<IRepositoryAsync<TripExpenseBudget>, Repository<TripExpenseBudget>>();
            container.RegisterType<IRepository<TripExpenseBudget>, Repository<TripExpenseBudget>>();

            container.RegisterType<IContactService, ContactService>();
            container.RegisterType<IRepositoryAsync<Contact>, Repository<Contact>>();
            container.RegisterType<IRepository<Contact>, Repository<Contact>>();

            container.RegisterType<IScheduleLogService, ScheduleLogService>();
            container.RegisterType<IRepositoryAsync<ScheduleLog>, Repository<ScheduleLog>>();
            container.RegisterType<IRepository<ScheduleLog>, Repository<ScheduleLog>>();

            container.RegisterType<IJobLogService, JobLogService>();
            container.RegisterType<IRepositoryAsync<JobLog>, Repository<JobLog>>();
            container.RegisterType<IRepository<JobLog>, Repository<JobLog>>();

            container.RegisterType<IService<JobRetryLog>, Service<JobRetryLog>>();
            container.RegisterType<IRepositoryAsync<JobRetryLog>, Repository<JobRetryLog>>();
            container.RegisterType<IRepository<JobRetryLog>, Repository<JobRetryLog>>();

            container.RegisterType<IService<TemplateMaster>, Service<TemplateMaster>>();
            container.RegisterType<IRepositoryAsync<TemplateMaster>, Repository<TemplateMaster>>();
            container.RegisterType<IRepository<TemplateMaster>, Repository<TemplateMaster>>();

            container.RegisterType<IMessageAddressService, MessageAddressService>();
            container.RegisterType<IRepositoryAsync<MessageAddress>, Repository<MessageAddress>>();
            container.RegisterType<IRepository<MessageAddress>, Repository<MessageAddress>>();

            container.RegisterType<IApiPubSubStoreService, ApiPubSubStoreService>();
            container.RegisterType<IRepositoryAsync<ApiPubSubStore>, Repository<ApiPubSubStore>>();
            container.RegisterType<IRepository<ApiPubSubStore>, Repository<ApiPubSubStore>>();

            container.RegisterType<IPurchaseOrderLogService, PurchaseOrderLogService>();
            container.RegisterType<IRepositoryAsync<PurchaseOrderLog>, Repository<PurchaseOrderLog>>();
            container.RegisterType<IRepository<PurchaseOrderLog>, Repository<PurchaseOrderLog>>();

            container.RegisterType<ILoanService, LoanService>();
            container.RegisterType<IRepositoryAsync<Loan>, Repository<Loan>>();
            container.RegisterType<IRepository<Loan>, Repository<Loan>>();
            
            container.RegisterType<ILoanLogService, LoanLogService>();
            container.RegisterType<IRepositoryAsync<LoanLog>, Repository<LoanLog>>();
            container.RegisterType<IRepository<LoanLog>, Repository<LoanLog>>();

            container.RegisterType<IBillSubmissionService, BillSubmissionService>();
            container.RegisterType<IRepositoryAsync<BillSubmission>, Repository<BillSubmission>>();
            container.RegisterType<IRepository<BillSubmission>, Repository<BillSubmission>>();

            container.RegisterType<IOfficeVehicleMapService, OfficeVehicleMapService>();
            container.RegisterType<IRepositoryAsync<OfficeVehicleMap>, Repository<OfficeVehicleMap>>();
            container.RegisterType<IRepository<OfficeVehicleMap>, Repository<OfficeVehicleMap>>();

            container.RegisterType<ILedgerOfficeService, LedgerOfficeService>();
            container.RegisterType<IRepositoryAsync<LedgerOffice>, Repository<LedgerOffice>>();
            container.RegisterType<IRepository<LedgerOffice>, Repository<LedgerOffice>>();

            container.RegisterType<IGpsEndPointService, GpsEndPointService>();
            container.RegisterType<IRepositoryAsync<GpsEndPoint>, Repository<GpsEndPoint>>();
            container.RegisterType<IRepository<GpsEndPoint>, Repository<GpsEndPoint>>();

            container.RegisterType<IRepositoryAsync<IntrgrationServiceLog>, Repository<IntrgrationServiceLog>>();
            container.RegisterType<IRepository<IntrgrationServiceLog>, Repository<IntrgrationServiceLog>>();

            container.RegisterType<IPartyRouteTimeService, PartyRouteTimeService>();
            container.RegisterType<IRepositoryAsync<PartyRouteTime>, Repository<PartyRouteTime>>();
            container.RegisterType<IRepository<PartyRouteTime>, Repository<PartyRouteTime>>();

            container.RegisterType<ISalesLogService, SalesLogService>();
            container.RegisterType<IRepositoryAsync<SalesLog>, Repository<SalesLog>>();
            container.RegisterType<IRepository<SalesLog>, Repository<SalesLog>>();

            container.RegisterType<IGPSKmLogService, GPSKmLogService>();
            container.RegisterType<IRepositoryAsync<GPSKmLog>, Repository<GPSKmLog>>();
            container.RegisterType<IRepository<GPSKmLog>, Repository<GPSKmLog>>();

            container.RegisterType<IRouteVehicleTypeService, RouteVehicleTypeService>();
            container.RegisterType<IRepositoryAsync<RouteVehicleType>, Repository<RouteVehicleType>>();
            container.RegisterType<IRepository<RouteVehicleType>, Repository<RouteVehicleType>>();

            container.RegisterType<IRepositoryAsync<Rule>, Repository<Rule>>();
            container.RegisterType<IRepository<Rule>, Repository<Rule>>();

            container.RegisterType<ICardMasterService, CardMasterService>();
            container.RegisterType<IRepositoryAsync<CardMaster>, Repository<CardMaster>>();
            container.RegisterType<IRepository<CardMaster>, Repository<CardMaster>>();

            container.RegisterType<IVehicleCardMappingService, VehicleCardMappingService>();
            container.RegisterType<IRepositoryAsync<VehicleCardMapping>, Repository<VehicleCardMapping>>();
            container.RegisterType<IRepository<VehicleCardMapping>, Repository<VehicleCardMapping>>();

            container.RegisterType<IGSTConfigurationService, GSTConfigurationService>();
            container.RegisterType<IRepositoryAsync<GSTConfiguration>, Repository<GSTConfiguration>>();
            container.RegisterType<IRepository<GSTConfiguration>, Repository<GSTConfiguration>>();

            container.RegisterType<ICNExtraInfoService, CNExtraInfoService>();
            container.RegisterType<IRepositoryAsync<CNExtraInfo>, Repository<CNExtraInfo>>();
            container.RegisterType<IRepository<CNExtraInfo>, Repository<CNExtraInfo>>();

            container.RegisterType<IGenericRatechartService, GenericRatechartService>();
            container.RegisterType<IRepositoryAsync<VehicleConfigurationLog>, Repository<VehicleConfigurationLog>>();
            container.RegisterType<IRepository<VehicleConfigurationLog>, Repository<VehicleConfigurationLog>>();

            container.RegisterType<IRepositoryAsync<GPSStatusLog>, Repository<GPSStatusLog>>();
            container.RegisterType<IRepository<GPSStatusLog>, Repository<GPSStatusLog>>();

            container.RegisterType<IRepositoryAsync<TripScheduleConfiguration>, Repository<TripScheduleConfiguration>>();
            container.RegisterType<IRepository<TripScheduleConfiguration>, Repository<TripScheduleConfiguration>>();

            container.RegisterType<IMaterialLocationMappingService, MaterialLocationMappingService>();
            container.RegisterType<IRepositoryAsync<MaterialLocationMap>, Repository<MaterialLocationMap>>();
            container.RegisterType<IRepository<MaterialLocationMap>, Repository<MaterialLocationMap>>();

            container.RegisterType<IRepositoryAsync<UserReportCustomization>, Repository<UserReportCustomization>>();
            container.RegisterType<IRepository<UserReportCustomization>, Repository<UserReportCustomization>>();

            container.RegisterType<IRepositoryAsync<PartyContractMap>, Repository<PartyContractMap>>();
            container.RegisterType<IRepository<PartyContractMap>, Repository<PartyContractMap>>();

            container.RegisterType<IRepositoryAsync<SalesOrderRequest>, Repository<SalesOrderRequest>>();
            container.RegisterType<IRepository<SalesOrderRequest>, Repository<SalesOrderRequest>>();

            container.RegisterType<IRepositoryAsync<GeneralTransaction>, Repository<GeneralTransaction>>();
            container.RegisterType<IRepository<GeneralTransaction>, Repository<GeneralTransaction>>();

            container.RegisterType<IRepositoryAsync<GeneralTransLog>, Repository<GeneralTransLog>>();
            container.RegisterType<IRepository<GeneralTransLog>, Repository<GeneralTransLog>>();

            container.RegisterType<IRepositoryAsync<DueMaster>, Repository<DueMaster>>();
            container.RegisterType<IDueMasterService, DueMasterService>();

            container.RegisterType<IRepositoryAsync<CurrencyConversion>, Repository<CurrencyConversion>>();
            container.RegisterType<ICurrencyConversionService, CurrencyConversionService>();

            container.RegisterType<IRepositoryAsync<APLConfig>, Repository<APLConfig>>();
            container.RegisterType<IAPLConfigService, APLConfigService>();


            container.RegisterType<IRepositoryAsync<APLType>, Repository<APLType>>();
            container.RegisterType<IAPLTypeService, APLTypeService>();
            

            container.RegisterType<IAPLLogService, APLLogService>();
            container.RegisterType<IRepositoryAsync<APLLog>, Repository<APLLog>>();

            container.RegisterType<IAPLLogAnxService, APLLogAnxService>();
            container.RegisterType<IRepositoryAsync<APLLogAnx>, Repository<APLLogAnx>>();

            container.RegisterType<IAPLLogAnxLevelService, APLLogAnxLevelService>();
            container.RegisterType<IRepositoryAsync<APLLogAnxLevel>, Repository<APLLogAnxLevel>>();


            container.RegisterType<ITransactionSupportLogService, TransactionSupportLogService>();
            container.RegisterType<IRepositoryAsync<TransactionSupportLog>, Repository<TransactionSupportLog>>();
            container.RegisterType<IRepository<TransactionSupportLog>, Repository<TransactionSupportLog>>();
            #region CRM
            container.RegisterType<IRepositoryAsync<ServiceUnit>, Repository<ServiceUnit>>();
            container.RegisterType<IRepositoryAsync<ServiceMaster>, Repository<ServiceMaster>>();
            container.RegisterType<IRepositoryAsync<CustomerServiceRequestLog>, Repository<CustomerServiceRequestLog>>();
            container.RegisterType<IRepositoryAsync<CustomerServiceRequest>, Repository<CustomerServiceRequest>>();
            #endregion

        }

        private void ConfigureValidation(IUnityContainer unity)
        {
            unity.RegisterType<IValidator<SpareLog>, SpareLogValidator>(new HierarchicalLifetimeManager());
        }
    }
}
