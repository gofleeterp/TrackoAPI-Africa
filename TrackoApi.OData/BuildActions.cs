using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using System;
using System.Web.OData.Builder;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Reporting.Models;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.ViewModels.FMS.Battery;
using TrackoAPI.ViewModels.FMS.Repairs;
using TrackoAPI.ViewModels.FMS.Tyres;
using TrackoAPI.ViewModels.Global;

namespace TrackoApi.OData
{
    public static class BuildActions
    {
        public static void RegisterFms(ODataConventionModelBuilder builder)
        {
            //odata/ExpenseMasters/AlterExpenseMasterStatus({ids})
            var alterEMStatus = builder.Action("AlterExpenseMasterStatus");
            alterEMStatus.Parameter<string>("ids");
            alterEMStatus.OptionalReturn = true;
            //odata/ExpenseMasters/AlterExpenseMasterStatus({ids})
            var alterDTStatus = builder.Action("AlterDueTypesStatus");
            alterDTStatus.Parameter<string>("ids");
            alterDTStatus.OptionalReturn = true;
            //odata/CreatePrepiadTax
            var createPrepaidTaxEntry = builder.Action("CreatePrepiadTax");
            createPrepaidTaxEntry.Parameter<long>("key");
            createPrepaidTaxEntry.OptionalReturn = true;

            //odata/AlterSparePartStatus({ids})
            var alterSPStatus = builder.Action("AlterSparePartStatus");
            alterSPStatus.Parameter<string>("ids");
            alterSPStatus.OptionalReturn = true;

            //odata/AlterORMLogStatus({ids})
            var alterORMStatus = builder.Action("AlterORMLogStatus");
            alterORMStatus.Parameter<string>("ids");
            alterORMStatus.OptionalReturn = true;

            //odata/AlterORMLogStatus({ids})
            var alterORMAStatus = builder.Action("AlterORMAuditLogStatus");
            alterORMAStatus.Parameter<string>("ids");
            alterORMAStatus.OptionalReturn = true;

            //odata/PostSparePurchaseBill(bill)
            var postSpareMaterialMRN = builder.Action("PostSpareMRN");
            postSpareMaterialMRN.Parameter<vwSparePurchaseBill>("bill");
            postSpareMaterialMRN.Returns<long>();
            postSpareMaterialMRN.OptionalReturn = true;

            //odata/PostSparePurchaseBill(bill)
            var postSparePurchaseBill = builder.Action("PostSparePurchaseBill");
            postSparePurchaseBill.Parameter<vwSparePurchaseBill>("bill");
            postSparePurchaseBill.Returns<long>();
            postSparePurchaseBill.OptionalReturn = true;

            //odata/PostMaterialDeliveryChallan(bill)
            var postSparedeliverychallan = builder.Action("PostMaterialDeliveryChallan");
            postSparedeliverychallan.Parameter<vwSparePurchaseBill>("bill");
            postSparedeliverychallan.Returns<long>();
            postSparedeliverychallan.OptionalReturn = true;            

            //odata/PostPurchaseBillSettlement(bill)
            var postPurchaseBillSettlement = builder.Action("PostPurchaseBillSettlement");
            postPurchaseBillSettlement.Parameter<vwSparePurchaseBill>("bill");
            postPurchaseBillSettlement.Returns<long>();
            postPurchaseBillSettlement.OptionalReturn = true;

            //odata/PostSpareConsumeBill(bill)
            var postSpareConsumeBill = builder.Action("PostSpareConsumeBill");
            postSpareConsumeBill.Parameter<vwSparePurchaseBill>("bill");
            postSpareConsumeBill.Parameter<long>("procid").OptionalParameter = true;
            postSpareConsumeBill.Returns<long>();
            postSpareConsumeBill.OptionalReturn = true;
            //odata/PostSpareIssueTransaction(bill)
            var postSpareIssueTransaction = builder.Action("PostSpareIssueTransaction");
            postSpareIssueTransaction.Parameter<vwSparePurchaseBill>("bill");
            postSpareIssueTransaction.Returns<long>();
            postSpareIssueTransaction.OptionalReturn = true;
            
            //odata/PostSpareIssueTransaction(bill)
            var postSpareOutwardTransaction = builder.Action("PostSpareOutwardTransaction");
            postSpareOutwardTransaction.Parameter<vwSparePurchaseBill>("bill");
            postSpareOutwardTransaction.Returns<long>();
            postSpareOutwardTransaction.OptionalReturn = true;

            //DeleteSpareTransaction
            //odata / DeleteSpareTransaction(Id)
            var deleteSpareTransaction = builder.Action("DeleteSpareTransaction");
            deleteSpareTransaction.Parameter<long>("key");
            deleteSpareTransaction.OptionalReturn = true;
            //PostStockTransferAcknowledgment
            //odata / PostStockTransferAcknowledgment(bill)
            var postStockTransferAcknowledgment = builder.Action("PostStockTransferAcknowledgment");
            postStockTransferAcknowledgment.Parameter<vwSparePurchaseBill>("bill");
            postStockTransferAcknowledgment.Returns<long>();
            postStockTransferAcknowledgment.OptionalReturn = true;

            //odata/PostSpareIssueTransaction(bill)
            var postTyrePurchaseTransaction = builder.Action("PostTyrePurchaseBill");
            postTyrePurchaseTransaction.Parameter<vwTyreBillView>("bill");
            postTyrePurchaseTransaction.Returns<long>();
            postTyrePurchaseTransaction.OptionalReturn = true;

            //odata/PostSpareIssueTransaction(bill)
            var postChasisTyreBill = builder.Action("PostChasisTyreBill");
            postChasisTyreBill.Parameter<vwTyreChassisBill>("bill");
            postChasisTyreBill.Returns<long>();
            postChasisTyreBill.OptionalReturn = true;

            //odata/PostSpareIssueTransaction(bill)
            var postTyreIssueReceiptBill = builder.Action("PostTyreIssueReceiptBill");
            postTyreIssueReceiptBill.Parameter<vwTyreBillView>("bill");
            postTyreIssueReceiptBill.Returns<long>();
            postTyreIssueReceiptBill.OptionalReturn = true;

            //odata/PostTyreIssueBill(bill)
            var postTyreIssueBill = builder.Action("PostTyreIssueBill");
            postTyreIssueBill.Parameter<vwTyreBillView>("bill");
            postTyreIssueBill.Returns<long>();
            postTyreIssueBill.OptionalReturn = true;

            //odata/PostTyreReceiptBill(bill)
            var postTyreReceiptBill = builder.Action("PostTyreReceiptBill");
            postTyreReceiptBill.Parameter<vwTyreBillView>("bill");
            postTyreReceiptBill.Returns<long>();
            postTyreReceiptBill.OptionalReturn = true;
            
            //odata/PostTyreResaleBill(bill)
            var postTyreResaleBill = builder.Action("PostTyreResaleBill");
            postTyreResaleBill.Parameter<vwTyreBillView>("bill");
            postTyreResaleBill.Returns<long>();
            postTyreResaleBill.OptionalReturn = true;

            //odata/PostTyreClaimRemouldBill(bill)
            var postTyreClaimRemouldBill = builder.Action("PostTyreClaimRemouldBill");
            postTyreClaimRemouldBill.Parameter<vwTyreBillView>("bill");
            postTyreClaimRemouldBill.Returns<long>();
            postTyreClaimRemouldBill.OptionalReturn = true;

            //odata/PostTyreScrapBill(bill)
            var postScrapBill = builder.Action("PostTyreScrapBill");
            postScrapBill.Parameter<vwTyreBillView>("bill");
            postScrapBill.Returns<long>();
            postScrapBill.OptionalReturn = true;

            //odata/PostTyreStocktransferOutBill(bill)
            var postTyreStocktransferOutBill = builder.Action("PostTyreStocktransferOutBill");
            postTyreStocktransferOutBill.Parameter<vwTyreBillView>("bill");
            postTyreStocktransferOutBill.Returns<long>();
            postTyreStocktransferOutBill.OptionalReturn = true;

            //odata/PostTyreStocktransferInBill(bill)
            var postTyreStocktransferInBill = builder.Action("PostTyreStocktransferInBill");
            postTyreStocktransferInBill.Parameter<vwTyreBillView>("bill");
            postTyreStocktransferInBill.Returns<long>();
            postTyreStocktransferInBill.OptionalReturn = true;


            //odata/PostTyreRejectBill(bill)
            var postTyreRejectBill = builder.Action("PostTyreRejectBill");
            postTyreRejectBill.Parameter<vwTyreBillView>("bill");
            postTyreRejectBill.Returns<long>();
            postTyreRejectBill.OptionalReturn = true;

            //odata/PostTyreRemouldReceiptBill(bill)
            var postTyreRemouldReceiptBill = builder.Action("PostTyreRemouldReceiptBill");
            postTyreRemouldReceiptBill.Parameter<vwTyreBillView>("bill");
            postTyreRemouldReceiptBill.Returns<long>();
            postTyreRemouldReceiptBill.OptionalReturn = true;

            //odata/PostTyreClaimReceiptBill(bill)
            var postTyreClaimReceiptBill = builder.Action("PostTyreClaimReceiptBill");
            postTyreClaimReceiptBill.Parameter<vwTyreBillView>("bill");
            postTyreClaimReceiptBill.Returns<long>();
            postTyreClaimReceiptBill.OptionalReturn = true;

            //odata/PostTyreClaimSettlementBill(bill)
            var postTyreClaimSettlementBill = builder.Action("PostTyreClaimSettlementBill");
            postTyreClaimSettlementBill.Parameter<vwTyreBillView>("bill");
            postTyreClaimSettlementBill.Returns<long>();
            postTyreClaimSettlementBill.OptionalReturn = true;

            //odata / DeleteSpareTransaction(Id)
            var deleteTyreBill = builder.Action("DeleteTyreTransaction");
            deleteTyreBill.Parameter<long>("key");
            deleteTyreBill.OptionalReturn = true;

            var cnPartialUpdate = builder.EntityType<CNMaster>().Action("CNPartialUpdate");
            cnPartialUpdate.Parameter<CNMaster>("cn");
            cnPartialUpdate.OptionalReturn = true;

            //odata / DeleteSpareTransaction(Id)
            var updateArrival = builder.EntityType<CnChallan>().Action("UpdateArrival");
            updateArrival.Parameter<decimal>("ArrivalQty");
            updateArrival.Parameter<decimal>("ShortQty");
            updateArrival.Parameter<decimal>("ExcessQty");
            updateArrival.Parameter<DateTime?>("ArrivalDate");
            updateArrival.OptionalReturn = true;
            var updateDeliveryFailed = builder.EntityType<CnChallan>().Action("UpdateDeliveryFailed");
            updateDeliveryFailed.Parameter<DateTime?>("DeliveryFailedDate");
            updateDeliveryFailed.OptionalReturn = true;

            //
            var updateTLFreight = builder.EntityType<VehicleMovementLog>().Action("RecalculateTLFreight");
            updateTLFreight.OptionalReturn = true;

            var updateVoucherAudit = builder.EntityType<Voucher>().Action("UpdateAuditStatus");
            updateVoucherAudit.Parameter<int>("isAudited");
            updateVoucherAudit.Parameter<string>("remark");
            updateVoucherAudit.OptionalReturn = true;

            //Battery Module
            //odata/PostSpareIssueTransaction(bill)
            var postBatteryPurchaseTransaction = builder.Action("PostBatteryPurchaseBill");
            postBatteryPurchaseTransaction.Parameter<vwBatteryBillView>("bill");
            postBatteryPurchaseTransaction.OptionalReturn = true;

            //odata/PostSpareIssueTransaction(bill)
            var postChasisBatteryBill = builder.Action("PostChasisBatteryBill");
            postChasisBatteryBill.Parameter<vwBatteryChassisBill>("bill");
            postChasisBatteryBill.OptionalReturn = true;

            //odata/PostSpareIssueTransaction(bill)
            var postBatteryIssueReceiptBill = builder.Action("PostBatteryIssueReceiptBill");
            postBatteryIssueReceiptBill.Parameter<vwBatteryBillView>("bill");
            postBatteryIssueReceiptBill.OptionalReturn = true;

            //odata/PostSpareIssueTransaction(bill)
            var postBatteryResaleBill = builder.Action("PostBatteryResaleBill");
            postBatteryResaleBill.Parameter<vwBatteryBillView>("bill");
            postBatteryResaleBill.OptionalReturn = true;

            //odata/PostBatteryClaimRemouldBill(bill)
            var postBatteryClaimRemouldBill = builder.Action("PostBatteryClaimRefurbishBill");
            postBatteryClaimRemouldBill.Parameter<vwBatteryBillView>("bill");
            postBatteryClaimRemouldBill.OptionalReturn = true;

            //odata/PostBatteryScrapBill(bill)
            var postBatteryScrapBill = builder.Action("PostBatteryScrapBill");
            postBatteryScrapBill.Parameter<vwBatteryBillView>("bill");
            postBatteryScrapBill.OptionalReturn = true;

            //odata/PostBatteryStocktransferOutBill(bill)
            var postBatteryStocktransferOutBill = builder.Action("PostBatteryStocktransferOutBill");
            postBatteryStocktransferOutBill.Parameter<vwBatteryBillView>("bill");
            postBatteryStocktransferOutBill.OptionalReturn = true;

            //odata/PostBatteryStocktransferInBill(bill)
            var postBatteryStocktransferInBill = builder.Action("PostBatteryStocktransferInBill");
            postBatteryStocktransferInBill.Parameter<vwBatteryBillView>("bill");
            postBatteryStocktransferInBill.OptionalReturn = true;


            //odata/PostBatteryRejectBill(bill)
            var postBatteryRejectBill = builder.Action("PostBatteryRejectBill");
            postBatteryRejectBill.Parameter<vwBatteryBillView>("bill");
            postBatteryRejectBill.OptionalReturn = true;

            //odata/PostBatteryRejectBill(bill)
            var postBatteryRemouldReceiptBill = builder.Action("PostBatteryRefurbishReceiptBill");
            postBatteryRemouldReceiptBill.Parameter<vwBatteryBillView>("bill");
            postBatteryRemouldReceiptBill.OptionalReturn = true;

            //odata/PostBatteryRejectBill(bill)
            var postBatteryClaimReceiptBill = builder.Action("PostBatteryClaimReceiptBill");
            postBatteryClaimReceiptBill.Parameter<vwBatteryBillView>("bill");
            postBatteryClaimReceiptBill.OptionalReturn = true;

            //odata/PostBatteryRejectBill(bill)
            var postBatteryClaimSettlement = builder.Action("PostBatteryClaimSettlement");
            postBatteryClaimSettlement.Parameter<vwBatteryBillView>("bill");
            postBatteryClaimSettlement.OptionalReturn = true;

            //odata/PostBatteryIssueBill(bill)
            var postBatteryIssueBill = builder.Action("PostBatteryIssueBill");
            postBatteryIssueBill.Parameter<vwBatteryBillView>("bill");
            postBatteryIssueBill.OptionalReturn = true;

            //odata/PostBatteryReceiptBill(bill)
            var postBatteryReceiptBill = builder.Action("PostBatteryReceiptBill");
            postBatteryReceiptBill.Parameter<vwBatteryBillView>("bill");
            postBatteryReceiptBill.OptionalReturn = true;


            //odata / DeleteSpareTransaction(Id)
            var deleteBatteryBill = builder.Action("DeleteBatteryTransaction");
            deleteBatteryBill.Parameter<long>("key");
            deleteBatteryBill.OptionalReturn = true;
            //odata / DeleteSpareTransaction(Id)
            var updateBookMapping = builder.Action("UpdateBookMapping");
            updateBookMapping.CollectionParameter<vwStationaryMapping>("books");
            updateBookMapping.OptionalReturn = true;
            //
            // actions
            //BuildAction(model, "PrimitiveAction", entityType, "param", intType);
            ActionConfiguration verifyDataIntegration = builder.EntityType<CNBill>().Collection.Action("VerifyDataIntegration");
            verifyDataIntegration.Parameter<string>("Ids");
            verifyDataIntegration.Returns<string>();
            verifyDataIntegration.OptionalReturn = true;

            var cnbulkupload = builder.EntityType<CNMaster>().Collection.Action("BulkPost");
            cnbulkupload.CollectionParameter<CNMaster>("cns");
            cnbulkupload.Returns<vwBatch>();
            cnbulkupload.OptionalReturn = true;

            var gebulkupload = builder.EntityType<vwGeneralExpenseVoucher>().Collection.Action("BulkPost");
            gebulkupload.CollectionParameter<vwGeneralExpenseVoucher>("vouchers");
            gebulkupload.Returns<vwBatch>();
            gebulkupload.OptionalReturn = true;

            var schedulebulkpost = builder.EntityType<TripScheduleConfiguration>().Collection.Action("BulkPost");
            schedulebulkpost.CollectionParameter<TripScheduleConfiguration>("logs");
            schedulebulkpost.OptionalReturn = true;

            var triplogdeepinsert = builder.EntityType<VehicleMovementLog>().Collection.Action("DeepPost");
            triplogdeepinsert.Parameter<VehicleMovementLog>("entity");
            triplogdeepinsert.OptionalReturn = true;

            var tripadvancebulkupload = builder.EntityType<vwAdvanceVoucher>().Collection.Action("BatchPostAdvances");
            tripadvancebulkupload.CollectionParameter<vwAdvanceVoucher>("vouchers");
            tripadvancebulkupload.Returns<vwBatch>();
            tripadvancebulkupload.OptionalReturn = true;

            var BulkAdvanceWithVoucher = builder.Action("BulkAdvanceWithVoucher");
            BulkAdvanceWithVoucher.CollectionParameter<TripAdvanceLog>("advances");
            BulkAdvanceWithVoucher.Parameter<string>("voucher");
            BulkAdvanceWithVoucher.Parameter<long>("procid").OptionalParameter = true; ;
            BulkAdvanceWithVoucher.Returns<long>();
            BulkAdvanceWithVoucher.OptionalReturn = true;

            var cnbulkupload100 = builder.EntityType<CNMaster>().Collection.Action("BulkPost100");
            cnbulkupload100.CollectionParameter<CNMaster>("cns");
            cnbulkupload100.Returns<vwBatch>();
            cnbulkupload100.OptionalReturn = true;

            var genericMasterbulkupload = builder.EntityType<GenericMaster>().Collection.Action("BulkPostGeneric");
            genericMasterbulkupload.CollectionParameter<GenericMaster>("masters");
            genericMasterbulkupload.Returns<vwBatch>();
            genericMasterbulkupload.OptionalReturn = true;

            var vehicleMasterbulkupload = builder.EntityType<VehicleMaster>().Collection.Action("BulkPostVehicle");
            vehicleMasterbulkupload.CollectionParameter<VehicleMaster>("vehicles");
            vehicleMasterbulkupload.Returns<vwBatch>();
            vehicleMasterbulkupload.OptionalReturn = true;

            var driverMasterbulkupload = builder.EntityType<DriverMaster>().Collection.Action("BulkPostDriver");
            driverMasterbulkupload.CollectionParameter<DriverMaster>("drivers");
            driverMasterbulkupload.Returns<vwBatch>();
            driverMasterbulkupload.OptionalReturn = true;

            var ledgerMasterbulkupload = builder.EntityType<Ledger>().Collection.Action("BulkPostLedger");
            ledgerMasterbulkupload.CollectionParameter<Ledger>("ledgers");
            ledgerMasterbulkupload.Returns<vwBatch>();
            ledgerMasterbulkupload.OptionalReturn = true;

            var triplogbulkupload = builder.EntityType<VehicleMovementLog>().Collection.Action("BulkPostTripLog");
            triplogbulkupload.CollectionParameter<VehicleMovementLog>("trps");
            triplogbulkupload.Returns<vwBatch>();
            triplogbulkupload.OptionalReturn = true;

            var bulkAdvanceUpload = builder.EntityType<vwAdvanceVoucher>().Collection.Action("BulkPostAdvances");
            bulkAdvanceUpload.CollectionParameter<vwAdvanceVoucher>("vouchers");
            bulkAdvanceUpload.Returns<vwBatch>();
            bulkAdvanceUpload.OptionalReturn = true;

            var acknolodgeAllMessages = builder.EntityType<ApiPubSubStore>().Collection.Action("AcknolodgeAllMessages");
            acknolodgeAllMessages.Parameter<long>("typeId");
            acknolodgeAllMessages.CollectionParameter<long>("ids");
            acknolodgeAllMessages.OptionalReturn = true;

            //PutSrvAcknowledgement(list)
            var postSrvAcknowledgement = builder.EntityType<CNStockMMLog>().Collection.Action("SRVUpdate");
            postSrvAcknowledgement.CollectionParameter<VW_DispatchAcknowledgment>("srvlist");
            postSrvAcknowledgement.OptionalReturn = true;

            var contractratelog = builder.EntityType<CNRateContractLog>().Collection.Action("BulkPost");
            contractratelog.CollectionParameter<CNRateContractLog>("contractlogs");
            contractratelog.Returns<vwBatch>();
            contractratelog.OptionalReturn = true;

            var repairexpensesbulkupload = builder.EntityType<SpareLog>().Collection.Action("PostBulkRepairExpenses");
            repairexpensesbulkupload.CollectionParameter<vwSparePurchaseBill>("expenses");
            repairexpensesbulkupload.Returns<vwBatch>();
            repairexpensesbulkupload.OptionalReturn = true;

            var amcrexpensesbulkupload = builder.EntityType<SpareLog>().Collection.Action("PostBulkAmcExpenses");
            amcrexpensesbulkupload.CollectionParameter<vwSparePurchaseBill>("amcexpenses");
            amcrexpensesbulkupload.Returns<vwBatch>();
            amcrexpensesbulkupload.OptionalReturn = true;

            var amcpayment = builder.EntityType<SpareLog>().Collection.Action("AMCPayment");
            amcpayment.Parameter<string>("vouchers");
            amcpayment.CollectionParameter<long>("extrainfoids");
            amcpayment.CollectionParameter<long>("logids");
            amcpayment.OptionalReturn = true;
            var getReportV2 = builder.EntityType<ReportRequestPool>().Collection.Action("GetReportV2");
            getReportV2.OptionalReturn = true;
            getReportV2.Parameter<ReportRequestPool>("request");
            getReportV2.Returns<string>();

            var httpBulkPost = builder.EntityType<HttpRequestPool>().Collection.Action("BulkPost");
            httpBulkPost.OptionalReturn = false;
            httpBulkPost.Parameter<bool>("getresponse");
            httpBulkPost.CollectionParameter<HttpRequestPool>("entities");
            httpBulkPost.Returns<string>();

            var checkReferenceFlags = builder.EntityType<Ledger>().Collection.Action("CheckReferenceFlags");
            checkReferenceFlags.OptionalReturn = true;
            checkReferenceFlags.ReturnsCollection<long>();
            checkReferenceFlags.CollectionParameter<long>("ids");
            var tripsettlementPostv2 = builder.EntityType<VehicleTripSettlement>().Collection.Action("PostV2");
            tripsettlementPostv2.OptionalReturn = true;
            tripsettlementPostv2.Returns<long>();
            tripsettlementPostv2.Parameter<VehicleTripSettlement>("entity");

            var hiresettlementv1 = builder.EntityType<VehicleTripSettlement>().Collection.Action("PostHireSettlementV1");
            hiresettlementv1.OptionalReturn = true;
            hiresettlementv1.Returns<long>();
            hiresettlementv1.Parameter<VehicleTripSettlement>("entity");

        }
        

        public static void RegisterAMS(ODataConventionModelBuilder builder)
        {
            
        }
        public static void BuildAction(EdmModel model, string actName, IEdmEntityTypeReference bindingType, string paramName, IEdmTypeReference edmType)
        {
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);

            EdmAction boundAction = new EdmAction("NS", actName, returnType, isBound: true, entitySetPathExpression: null);
            boundAction.AddParameter("entity", bindingType);
            boundAction.AddParameter(paramName, edmType);
            boundAction.AddParameter(paramName + "List", new EdmCollectionTypeReference(new EdmCollectionType(edmType)));
            model.AddElement(boundAction);
        }
    }
}
