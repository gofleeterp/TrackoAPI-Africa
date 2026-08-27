using System;
using System.Collections.Generic;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Reporting.Models;
using TrackoAPI.Reports.ViewModels.Global;
using TrackoAPI.vw.ts;
using TrackoAPI.ViewModels;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.ViewModels.FMS.Battery;
using TrackoAPI.ViewModels.FMS.Repairs;
using TrackoAPI.ViewModels.FMS.Tyres;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.OData
{
    public class BuildFunctions
    {
        public static void RegisterFMS(ODataConventionModelBuilder builder)
        {
            var getFuelExpanses = builder.EntityType<TripAdvanceLog>().Collection.Function("GetFuelExpanses");
            getFuelExpanses.Parameter<long?>("settlementId");
            getFuelExpanses.Parameter<string>("tripLogIds");
            getFuelExpanses.ReturnsCollection<TripFuelExpense>();
           

            //odata/GetSparePurchaseBill(Id)
            var getsparePurchaseBill = builder.Function("GetSparePurchaseBill");
            getsparePurchaseBill.Parameter<long>("key");
            getsparePurchaseBill.Returns<vwSparePurchaseBill>();

            //odata/GetPurchaseBillSettlment(Id)
            var getPurchaseBillSettlment = builder.Function("GetPurchaseBillSettlment");
            getPurchaseBillSettlment.Parameter<long>("key");
            getPurchaseBillSettlment.Returns<vwSparePurchaseBill>();

            //odata/GetSpareConsumeBill(Id)
            var getspareConsumeBill = builder.Function("GetSpareConsumeBill");
            getspareConsumeBill.Parameter<long>("key");
            getspareConsumeBill.Returns<vwSparePurchaseBill>();

            //odata/GetSpareIssueTransaction(Id)
            var getSpareIssueTransaction = builder.Function("GetSpareIssueTransaction");
            getSpareIssueTransaction.Parameter<long>("key");
            getSpareIssueTransaction.Returns<vwSparePurchaseBill>();

            //odata/GetSpareOutwardTransaction(Id)
            var getSpareOutwardTransaction = builder.Function("GetSpareOutwardTransaction");
            getSpareOutwardTransaction.Parameter<long>("key");
            getSpareOutwardTransaction.Returns<vwSparePurchaseBill>();

            //odata/GetStockTransferAcknowledgment(Id)
            var getStockTransferAcknowledgment = builder.Function("GetStockTransferAcknowledgment");
            getStockTransferAcknowledgment.Parameter<long>("key");
            getStockTransferAcknowledgment.Returns<vwSparePurchaseBill>();

            //odata/getTyrePurchaseBill(Id)
            var getTyrePurchaseBill = builder.Function("GetTyrePurchaseBill");
            getTyrePurchaseBill.Parameter<long>("key");
            getTyrePurchaseBill.Returns<vwTyreBillView>();

            //odata/getTyrePurchaseBill(Id)
            var getTyreResaleBill = builder.Function("GetTyreResaleBill");
            getTyreResaleBill.Parameter<long>("key");
            getTyreResaleBill.Returns<vwTyreBillView>();

            //odata/getTyrePurchaseBill(Id)
            var getTyreChassisBill = builder.Function("GetTyreChassisBill");
            getTyreChassisBill.Parameter<long>("key");
            getTyreChassisBill.Returns<vwTyreChassisBill>();

            //odata/GetReportingCategories
            var getReportingCategories = builder.Function("GetReportingCategories");
            getReportingCategories.SupportedInFilter = true;
            getReportingCategories.SupportedInOrderBy = true;
            getReportingCategories.ReturnsCollection<vwReportCategory>();

            //odata/GetCategoriesByReportId
            var getCategoriesByReportId = builder.Function("GetCategoriesByReportId");
            getCategoriesByReportId.SupportedInFilter = true;
            getCategoriesByReportId.SupportedInOrderBy = true;
            getCategoriesByReportId.Parameter<long>("reportId");
            getCategoriesByReportId.ReturnsCollectionFromEntitySet<ObjectCategory>("ObjectCategories");
            getCategoriesByReportId.OptionalReturn = true;
            //odata/GetCategoriesByReportId
            var getCategoriesByCustomReportId = builder.Function("GetCategoriesByCustomReportId");
            getCategoriesByCustomReportId.SupportedInFilter = true;
            getCategoriesByCustomReportId.SupportedInOrderBy = true;
            getCategoriesByCustomReportId.Parameter<long>("reportId");
            getCategoriesByCustomReportId.ReturnsCollectionFromEntitySet<ObjectCategory>("ObjectCategories");
            getCategoriesByCustomReportId.OptionalReturn = true;
            //odata/SearchLedgerByVoucherType
            var getLedgersByVoucherTypeId = builder.Function("SearchLedgerByVoucherType");
            getLedgersByVoucherTypeId.Parameter<long?>("voucherTypeId");
            getLedgersByVoucherTypeId.Parameter<long>("fieldId");
            getLedgersByVoucherTypeId.Parameter<long?>("viewId");
            getLedgersByVoucherTypeId.SupportedInFilter = true;
            getLedgersByVoucherTypeId.SupportedInOrderBy = true;
            getLedgersByVoucherTypeId.ReturnsCollectionFromEntitySet<Ledger>("Ledgers");
            getLedgersByVoucherTypeId.OptionalReturn = true;

            var getNextStatusByDriverId = builder.Function("GetNextStatusByDriverId");
            getNextStatusByDriverId.Parameter<long>("driverId");
            getNextStatusByDriverId.Parameter<long>("vehicleId");
            getNextStatusByDriverId.SupportedInFilter = true;
            getNextStatusByDriverId.SupportedInOrderBy = true;
            getNextStatusByDriverId.ReturnsCollectionFromEntitySet<DriverNextStatusMapping>("DriverNextStatusMappings");
            getNextStatusByDriverId.OptionalReturn = true;

            var searchUsers = builder.Function("SearchUserNames");
            searchUsers.Parameter<string>("searchTerm");
            searchUsers.ReturnsCollection<vwUserName>();

            var searchRoles = builder.Function("SearchRoles");
            searchRoles.Parameter<string>("searchTerm");
            searchRoles.ReturnsCollection<vwRole>();

            //odata/getTyreClaimRemouldBill(Id)
            var getTyreClaimRemouldBill = builder.Function("GetTyreClaimRemouldBill");
            getTyreClaimRemouldBill.Parameter<long>("key");
            getTyreClaimRemouldBill.Returns<vwTyreBillView>();

            //odata/GetTyreScrapBill(Id)
            var getTyreScrapBill = builder.Function("GetTyreScrapBill");
            getTyreScrapBill.Parameter<long>("key");
            getTyreScrapBill.Returns<vwTyreBillView>();

            //odata/GetTyreStoretransferOutBill(Id)
            var getTyreStoretransferOutBill = builder.Function("GetTyreStoretransferOutBill");
            getTyreStoretransferOutBill.Parameter<long>("key");
            getTyreStoretransferOutBill.Returns<vwTyreBillView>();

            //odata/GetTyreStoretransferInBill(Id)
            var getTyreStoretransferInBill = builder.Function("GetTyreStoretransferInBill");
            getTyreStoretransferInBill.Parameter<long>("key");
            getTyreStoretransferInBill.Returns<vwTyreBillView>();

            //odata/GetTyreStoretransferInBill(Id)
            var getTyreRejectBill = builder.Function("GetTyreRejectBill");
            getTyreRejectBill.Parameter<long>("key");
            getTyreRejectBill.Returns<vwTyreBillView>();

            //odata/GetTyreStoretransferInBill(Id)
            var getTyreRemouldReceiptBill = builder.Function("GetTyreRemouldReceiptBill");
            getTyreRemouldReceiptBill.Parameter<long>("key");
            getTyreRemouldReceiptBill.Returns<vwTyreBillView>();

            //odata/GetStationaryByFieldId(Id)
            var getStationaryByFieldId = builder.Function("GetStationaryByFieldId");
            getStationaryByFieldId.Parameter<long>("fieldId");
            getStationaryByFieldId.Parameter<long?>("typeId").OptionalParameter=true;
            getStationaryByFieldId.Parameter<long?>("viewId").OptionalParameter = true;
            getStationaryByFieldId.ReturnsCollectionFromEntitySet<StationeryBookLog>("StationeryBookLogs");

            //Battery Module by Sanjay

            //odata/getBatteryPurchaseBill(Id)
            var getBatteryPurchaseBill = builder.Function("GetBatteryPurchaseBill");
            getBatteryPurchaseBill.Parameter<long>("key");
            getBatteryPurchaseBill.Returns<vwBatteryBillView>();

            //odata/getBatteryPurchaseBill(Id)
            var getBatteryResaleBill = builder.Function("GetBatteryResaleBill");
            getBatteryResaleBill.Parameter<long>("key");
            getBatteryResaleBill.Returns<vwBatteryBillView>();

            //odata/getBatteryPurchaseBill(Id)
            var getBatteryChassisBill = builder.Function("GetBatteryChassisBill");
            getBatteryChassisBill.Parameter<long>("key");
            getBatteryChassisBill.Returns<vwBatteryChassisBill>();

            //odata/getBatteryClaimRemouldBill(Id)
            var getBatteryClaimRemouldBill = builder.Function("GetBatteryClaimRefurbishBill");
            getBatteryClaimRemouldBill.Parameter<long>("key");
            getBatteryClaimRemouldBill.Returns<vwBatteryBillView>();

            //odata/GetBatteryScrapBill(Id)
            var getBatteryScrapBill = builder.Function("GetBatteryScrapBill");
            getBatteryScrapBill.Parameter<long>("key");
            getBatteryScrapBill.Returns<vwBatteryBillView>();

            //odata/GetBatteryStoretransferOutBill(Id)
            var getBatteryStoretransferOutBill = builder.Function("GetBatteryStoretransferOutBill");
            getBatteryStoretransferOutBill.Parameter<long>("key");
            getBatteryStoretransferOutBill.Returns<vwBatteryBillView>();

            //odata/GetBatteryStoretransferInBill(Id)
            var getBatteryStoretransferInBill = builder.Function("GetBatteryStoretransferInBill");
            getBatteryStoretransferInBill.Parameter<long>("key");
            getBatteryStoretransferInBill.Returns<vwBatteryBillView>();

            //odata/GetBatteryStoretransferInBill(Id)
            var getBatteryRejectBill = builder.Function("GetBatteryRejectBill");
            getBatteryRejectBill.Parameter<long>("key");
            getBatteryRejectBill.Returns<vwBatteryBillView>();

            //odata/GetBatteryStoretransferInBill(Id)
            var getBatteryRemouldReceiptBill = builder.Function("GetBatteryRefurbishReceiptBill");
            getBatteryRemouldReceiptBill.Parameter<long>("key");
            getBatteryRemouldReceiptBill.Returns<vwBatteryBillView>();
            // functions
            //BuildFunction(model, "PrimitiveFunction", entityType, "param", intType);
            var getImageUrls = builder.EntityType<ApiFile>().Collection.Function("GetImageUrls").ReturnsCollection<string>();
            getImageUrls.Parameter<long>("recordid");
            getImageUrls.Parameter<long>("typeid");

            var getrateContactDistinctLoadTypes =
                builder.EntityType<CNRateContractLog>().Function("GetDistinctLoadTypes");
            getrateContactDistinctLoadTypes.ReturnsCollectionFromEntitySet<LoadType>("LoadTypes");
            //GetScript
            var getLoadTypeScript =
                builder.EntityType<CNRateContractLog>().Function("GetScript");
            getLoadTypeScript.Returns<string>();

            var getTop10CNStock = builder.EntityType<CNStockLog>().Collection.Function("SearchTop10CNStock");
            getTop10CNStock.Parameter<long>("challanOfficeId");
            getTop10CNStock.Parameter<long>("stockOfficeId");
            getTop10CNStock.Parameter<DateTime?>("challanDate");
            getTop10CNStock.Parameter<string>("serachTerm");
            getTop10CNStock.ReturnsCollection<vwCNStockSearch>();
            

            var getTop10CNStockMMSearch = builder.EntityType<CNStockMMLog>().Collection.Function("SearchTop10CNStockMM");
            getTop10CNStockMMSearch.Parameter<long>("stockOfficeId");
            getTop10CNStockMMSearch.Parameter<DateTime>("stockDate");
            getTop10CNStockMMSearch.Parameter<string>("serachTerm");
            getTop10CNStockMMSearch.ReturnsCollectionFromEntitySet<CNStockMMLog>("CNStockMMLogs");

            var getUnsettledHSAdvances = builder.EntityType<HSAdvance>().Collection.Function("GetUnsettledHSAdvances");
            getUnsettledHSAdvances.ReturnsCollectionFromEntitySet<HSAdvance>("HSAdvances");

            var getUnsettledAdvances = builder.EntityType<TripAdvanceLog>().Collection.Function("GetUnsettledAdvances");
            getUnsettledAdvances.ReturnsCollectionFromEntitySet<TripAdvanceLog>("TripAdvanceLogs");

            var getReportSearch = builder.EntityType<ReportRequestPool>().Collection.Function("SearchReport");
            getReportSearch.Parameter<string>("searchTerm");
            getReportSearch.ReturnsCollection<vwReportSearch>();

            var searchHireOwnVehicle = builder.EntityType<VehicleMaster>().Collection.Function("GetOwnHireVehicleNew");
            searchHireOwnVehicle.Parameter<string>("searchTerm");
            searchHireOwnVehicle.Parameter<int>("count");
            searchHireOwnVehicle.ReturnsCollection<vwOwnHireVehicle>();

            var getPendingVDRs = builder.EntityType<VoucherDetailReference>().Collection.Function("GetPendingReferences");
            getPendingVDRs.ReturnsCollection<VDRBalance>();
            //getPendingVDRs.Parameter<string>("searchTerm");
            //getPendingVDRs.Parameter<int>("count");
            //getPendingVDRs.Parameter<long>("accountId");
            getPendingVDRs.SupportedInOrderBy = true;
            getPendingVDRs.SupportedInFilter = true;

            var getPendingCNBills = builder.EntityType<CNBill>().Collection.Function("GetPendingBills");
            getPendingCNBills.ReturnsCollectionFromEntitySet<CNBill>("CNBills");

            var getOwnHireVehicle = builder.EntityType<VehicleMaster>().Collection.Function("GetOwnHireVehicle");
            getOwnHireVehicle.ReturnsCollection<vwOwnHireVehicle>();
            getOwnHireVehicle.SupportedInOrderBy = true;
            getOwnHireVehicle.SupportedInFilter = true;

            var getclassobjects = builder.EntityType<ObjectClassMap>().Collection.Function("GetObjectMappings");
            getclassobjects.Parameter<string>("keys");
            getclassobjects.Parameter<int>("count");
            getclassobjects.Parameter<string>("searchTerm").OptionalParameter=true;
            getclassobjects.ReturnsCollectionFromEntitySet<ObjectClassMap>("ObjectClassMapping");
            getclassobjects.SupportedInOrderBy = true;
            getclassobjects.SupportedInFilter = true;

            var checkStationaryisused = builder.EntityType<StationeryBook>().Function("IsBookUsed");
            checkStationaryisused.OptionalReturn = true;
            checkStationaryisused.Returns<bool>();

            var DoesProcProvideJson = builder.EntityType<ReportRequestPool>().Collection.Function("DoesProcProvideJson");
            DoesProcProvideJson.Parameter<long>("reportId");
            DoesProcProvideJson.Parameter<long>("procId");
            DoesProcProvideJson.Returns<bool>();


        }
        public static void BuildFunction(EdmModel model, string funcName, IEdmEntityTypeReference bindingType, string paramName, IEdmTypeReference edmType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            EdmFunction boundFunction = new EdmFunction("NS", funcName, returnType, isBound: true, entitySetPathExpression: null, isComposable: false);
            boundFunction.AddParameter("entity", bindingType);
            boundFunction.AddParameter(paramName, edmType);
            boundFunction.AddParameter(paramName + "List", new EdmCollectionTypeReference(new EdmCollectionType(edmType)));
            model.AddElement(boundFunction);
        }
       
    }
}