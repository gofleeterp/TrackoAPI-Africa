using AutoMapper;
using EntityFramework.Extensions;
using Hangfire;
using Microsoft.TeamFoundation.SourceControl.WebApi.Legacy;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service.BMS;
using TrackoApi.Service.Global;
using TrackoApi.Service.TMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;
using TrackoAPI.WebUtilities.Helper;
using IsolationLevel = System.Data.IsolationLevel;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ConsignmentsController : ODataController
    {
        private readonly ICNMultiMaterialService _CnMMrepo;
        private readonly IMapper _mapper;
        private readonly IConsignmentsService _repo;
        public ConsignmentsController(IConsignmentsService service, ICNMultiMaterialService cnMultiMaterial, IMapper mapper)
        {
            _repo = service;
            _CnMMrepo = cnMultiMaterial;
            _mapper = mapper;
        }

        [HttpPost]
        public IHttpActionResult BulkPost(ODataActionParameters parameters)
        {
            try
            {
                var batchId = Guid.NewGuid().ToString("N");
                var icns = parameters["cns"] as IEnumerator<CNMaster>;
                if (icns == null) return BadRequest("No CN found to upload");
                var cns = icns.ToList();
                var uow = Request.GetContext();
                //var salesConfig = uow.Context.GetApiConfig<int>("GenerateSalesVoucher");
                //var salesLogs = new List<SalesLog>();
                //var salesVouchers = new List<Voucher>();
                var materials = new Dictionary<string, List<CNMultiMaterial>>();
                //var billingparties = cns.Select(x => x.BillingPartyId.GetValueOrDefault()).Distinct().ToArray();
                //var salesLedgers = await uow.Context.Ledgers.Where(x => billingparties.Contains(x.Id)).Select(x =>
                //    new
                //    {
                //        x.SalesAccountId,
                //        x.UnbilledSalesAcId,
                //        x.Id,
                //        x.AccountName
                //    }).ToListAsync();

                foreach (var entity in cns)
                {
                    entity.ObjectState = ObjectState.Added;
                    if (entity.CNTypeId == 1369)
                    {
                        entity.TripLogId = null;
                        entity.TLLoadQty = 0;
                    }
                    entity.ConstCurTypeId = Helper.ConstCurTypeId;
                    entity.CreatedDOE = DateTime.Now;
                    entity.CreatedSessionId = Helper.SessionId();
                    entity.BatchId = batchId;
                    //TODO:It was not clear how to map sales log to cn , and sales log to voucher,vd and vdr
                    //var sl = new SalesLog { BatchId = batchId };
                    //entity.PrepareSalesLog(ref sl);
                    //salesLogs.Add(sl);
                    //if (salesConfig != 1) continue;
                    //var slaccount = salesLedgers.FirstOrDefault(x => x.Id == sl.BillingPartyId.GetValueOrDefault());
                    //if (slaccount?.SalesAccountId == null || slaccount?.UnbilledSalesAcId == null)
                    //{
                    //    throw new BusinessException(ErrorCode.GLB106, $"Sales Account and Unabilled Sales Account are not Mapped to Billing Party for the CN {entity.CNNo}.");
                    //}
                    //sl.SalesAccountId = slaccount.SalesAccountId;
                    //sl.UnbilledSalesAcId = slaccount.UnbilledSalesAcId;
                    //var salesVoucher = new Voucher() { BatchId= batchId };
                    //sl.PrepareSalesVoucher(ref salesVoucher);
                    //salesVouchers.Add(salesVoucher);
                    if (entity.MultiMaterialsView!=null&&entity.MultiMaterialsView.Any())
                    {
                        materials.Add(entity.CNNo,entity.MultiMaterialsView.Select(c => new CNMultiMaterial
                        {
                            ActualQty=c.ActualQty,
                            ActualQtyUnitId=c.ActualQtyUnitId,
                            ActualWeight=c.ActualWeight,
                            ActualWeightUnitId=c.ActualWeightUnitId,
                            AddI=c.AddI,
                            AddII=c.AddII,
                            AddIII=c.AddIII,
                            AddIV = c.AddIV,
                            AddV = c.AddV,
                            AddVI = c.AddVI,
                            AddVII = c.AddVII,
                            AddVIII = c.AddVIII,
                            Breadth = c.Breadth,
                            CFT = c.CFT,
                            ChargeQty = c.ChargeQty,
                            ChargeQtyUnitId = c.ChargeQtyUnitId,
                            ChargeWeight = c.ChargeWeight,
                            ChargeWeightUnitId = c.ChargeWeightUnitId,
                            CreatedDOE=entity.CreatedDOE,
                            CreatedSessionId=entity.CreatedSessionId,
                            EWayBillMM = c.EWayBillMM,
                            eWayBillValidity = c.eWayBillValidity,
                            ExciseAmount = c.ExciseAmount,
                            ExciseRate = c.ExciseRate,
                            Freight = c.Freight,
                            Height = c.Height,
                            InvoiceDate = c.InvoiceDate,
                            InvoiceNetValue = c.InvoiceNetValue,
                            InvoiceNo = c.InvoiceNo,
                            InvoiceRate = c.InvoiceRate,
                            InvoiceValue = c.InvoiceValue,
                            Length = c.Length,
                            LessI = c.LessI,
                            LessII = c.LessII,
                            LessIII = c.LessIII,
                            LessIV = c.LessIV,
                            LessV = c.LessV,
                            LessVI = c.LessVI,
                            LessVII = c.LessVII,
                            LessVIII = c.LessVIII,
                            MaterialId = c.MaterialId,
                            NetFreight = c.NetFreight,
                            PkgUnitId = c.PkgUnitId,
                            
                            Rate = c.Rate,
                            Ref1 = c.Ref1,
                            Ref1Id = c.Ref1Id,
                            Ref2 = c.Ref2,
                            Ref3 = c.Ref3,
                            Ref4 = c.Ref3,
                            Remark = c.Remark,
                            ServiceTaxAmount = c.ServiceTaxAmount,
                            ServiceTaxRate = c.ServiceTaxRate,
                            VolumeUnitId = c.VolumeUnitId,
                            TotalPackage = c.TotalPackage
                        }).ToList());
                    }
                }
                var transaction = uow.Context.Database.CurrentTransaction ??
                                  uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {


                    uow.BulkInsert(cns, transaction.UnderlyingTransaction);
                    //if (salesLogs.Any())
                    //{
                    //    if (salesVouchers.Any())
                    //    {
                    //        uow.BulkInsert(salesVouchers);
                    //        var voucherids =
                    //    }
                    //    uow.BulkInsert(salesLogs);
                    //}
                    if (materials.Any())
                    {
                        var mts = new List<CNMultiMaterial>();
                        foreach (var mt in materials)
                        {
                            var cn = cns.FirstOrDefault(x => x.CNNo == mt.Key);
                            if (cn.Id > 0)
                            {
                                mt.Value.ForEach(x => x.CnId = cn.Id);
                                mts.AddRange(mt.Value);
                            }
                        }
                        if (mts.Any())
                        {
                            uow.BulkInsert(mts, transaction.UnderlyingTransaction);
                        }
                    }
                    if (!Request.IsBatchRequest())
                    {
                        transaction.Commit();
                        transaction.Dispose();
                    }
                }
                catch (Exception)
                {
                    if (!Request.IsBatchRequest())
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                    }
                    throw;
                }
                var item = new vwBatch { BatchId = batchId, BatchSize = cns.Count };
                return Ok(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> BulkPost100(ODataActionParameters parameters)
        {
            var batchId = Guid.NewGuid().ToString("N");
            var icns = parameters["cns"] as IEnumerator<CNMaster>;
            if (icns == null) return BadRequest("No CN found to upload");
            var cns = icns.ToList();
            if (cns.Count > 200)
                return BadRequest("Unable to Upload CNs as No of CN Exceed Max Number of Upload in Single Batch");
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            try
            {
                int batchcount = 0;
                foreach (var entity in cns)
                {
                    entity.ConstCurTypeId = Helper.ConstCurTypeId;
                    entity.ObjectState = ObjectState.Added;
                    if (entity.CNTypeId == 1369)
                    {
                        entity.TripLogId = null;
                        entity.TLLoadQty = 0;
                    }
                    entity.CreatedDOE = DateTime.Now;
                    entity.CreatedSessionId = Helper.SessionId();
                    entity.BatchId = batchId;
                    _repo.Insert(entity);
                    batchcount++;
                    if (batchcount == 50)
                    {
                        batchcount = 0;
                        await uow.SaveChangesAsync();
                    }
                }
                await uow.SaveChangesAsync();
                int tlbatchcount = 0;
                foreach (var cn in cns.Where(x => x.TripLogId > 0 && x.Id > 0))
                {
                    uow.Repository<VehicleMovementLog>().AttachCNToTripLog(cn.TripLogId.GetValueOrDefault(), cn.TripLogId.GetValueOrDefault(), cn);
                    tlbatchcount++;
                    if (tlbatchcount == 50)
                    {
                        tlbatchcount = 0;
                        await uow.SaveChangesAsync();
                    }
                    try
                    {
                        if (cn.TripLogId > 0)
                        {
                            var triplog = await uow.RepositoryAsync<VehicleMovementLog>().Queryable().FirstOrDefaultAsync(x => x.Id == cn.TripLogId);
                            var query = uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.TriplogId == cn.TripLogId.Value);
                            var totalfreight = (await query.SumAsync(x => (decimal?)x.fk_CNMaster.CNSubTotalII)) ?? 0;
                            var totalqty = (await query.SumAsync(x => (decimal?)x.Qty)) ?? 0;
                            triplog.CNFreight = totalfreight;
                            triplog.LoadingQty = totalqty;
                            triplog.ObjectState = triplog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                            //await uow.ExecSqlQueryAsync("UPDATE vml SET CNFreight=(SELECT SUM(cn.CNSubTotalII) FROM tCNChallan chn JOIN tCNMaster cn ON chn.CNId=cn.Id WHERE chn.TriplogId=vml.Id) FROM tVehicleMovementLog vml WHERE Id=@p0", new SqlParameter("p0", cn.TripLogId.Value));
                        }

                    }
                    catch
                    {
                        //Ignore
                    }
                }
                await uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            var item = new vwBatch { BatchId = batchId, BatchSize = cns.Count };
            return Ok(item);
        }

        [HttpPost]
        public async Task<IHttpActionResult> CNPartialUpdate([FromODataUri]long key, ODataActionParameters parameters)
        {
            var record = parameters["cn"] as CNMaster;
            if (record == null) return BadRequest("Please provide CN for Partial Update");
            var uow = Request.GetContext();
            uow.RepositoryAsync<CNMaster>().Attach(record);
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                record.ObjectState = ObjectState.Modified;
                var repo = uow.RepositoryAsync<SalesLog>();
                var sl = await repo.Queryable().FirstOrDefaultAsync(x => x.CNId == record.Id) ?? new SalesLog();
                record.PrepareSalesLog(ref sl);
                switch (sl.ObjectState)
                {
                    case ObjectState.Added:
                        if (record.CNTotalFreight > 0) repo.Insert(sl);
                        break;

                    case ObjectState.Modified:
                        if (record.CNTotalFreight > 0) repo.Update(sl);
                        break;

                    case ObjectState.Deleted:
                        repo.Delete(sl);
                        break;
                }
                var salesConfig = uow.Context.GetApiConfig<int>("GenerateSalesVoucher");
                if (salesConfig == 1)
                {
                    var salesLedgers = await uow.Context.Ledgers.Where(x => x.Id == record.BillingPartyId).Select(x =>
                        new
                        {
                            x.SalesAccountId,
                            x.UnbilledSalesAcId
                        }).FromCacheFirstOrDefaultAsync();
                    if (salesLedgers.SalesAccountId == null || salesLedgers.UnbilledSalesAcId == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Sales Account and Unabilled Sales Account are not Mapped to Billing Party.");
                    }

                    sl.SalesAccountId = salesLedgers.SalesAccountId;
                    sl.UnbilledSalesAcId = salesLedgers.UnbilledSalesAcId;
                }
                await uow.SaveChangesAsync();
                if (_repo.GetConfigValue<int>("RunCNPostProcess") == 1)
                {
                    BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunCNPostProcess(null,record.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(2));
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(record);
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
                string navigationProperty, [FromBody] Uri link)
        {

            var cn = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (cn == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_TripLog":
                    if (cn.CNTypeId == 1369)
                    {
                        cn.TripLogId = null;
                        cn.TLLoadQty = 0;
                    }
                    var tlRepo = uow.RepositoryAsync<VehicleMovementLog>();
                    var triplog =
                        await
                            tlRepo.Queryable().FirstOrDefaultAsync(x => x.Id == id);
                    if (triplog == null)
                    {
                        return NotFound();
                    }
                    var oldtlid = cn.TripLogId;
                    cn.TripLogId = id;
                    cn.TLLoadQty = cn.ActualQty;
                    cn.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    await tlRepo.AttachCNToTripLogAsync(id, oldtlid, cn.Id, cn);
                    try
                    {
                        var query = uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.TriplogId == cn.TripLogId.Value);
                        var totalfreight = (await query.SumAsync(x => (decimal?)x.fk_CNMaster.CNSubTotalII)) ?? 0;
                        var totalqty = (await query.SumAsync(x => (decimal?)x.Qty)) ?? 0;
                        triplog.CNFreight = totalfreight;
                        triplog.LoadingQty = totalqty;
                        triplog.ObjectState = triplog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        //await uow.ExecSqlQueryAsync("UPDATE vml SET CNFreight=(SELECT SUM(cn.CNSubTotalII) FROM tCNChallan chn JOIN tCNMaster cn ON chn.CNId=cn.Id WHERE chn.TriplogId=vml.Id) FROM tVehicleMovementLog vml WHERE Id=@p0", new SqlParameter("p0", cn.TripLogId.Value));
                    }
                    catch
                    {
                        //Ignore
                    }
                    break;

                case "fk_Bill":
                    var billrepo = uow.RepositoryAsync<CNBill>();
                    var bill =
                        await
                            billrepo.Queryable().AnyAsync(x => x.Id == id);
                    if (!bill)
                    {
                        return NotFound();
                    }
                    cn.BillId = id;
                    cn.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;

                case "fk_CnAdvanceId":
                    var cnadvanc = uow.RepositoryAsync<CNBillPayment>();
                    var cnadv =
                        await
                            cnadvanc.Queryable().AnyAsync(x => x.Id == id);
                    if (!cnadv)
                    {
                        return NotFound();
                    }
                    cn.CnAdvanceId = id;
                    cn.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            await uow.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var cnMaster = await _repo.FindAsync(key);
            if (cnMaster == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var ispodexist = await uow.RepositoryAsync<CNExtraInfo>().Queryable().AnyAsync(x => x.CNId==cnMaster.Id);
            if (ispodexist)
            {
                return BadRequest("POD has been received against this CN,so you cannot delete this CN");
            }
            await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tSalesLog] SET VDRId=NULL WHERE CNId={key}");
            await uow.ExecSqlQueryAsync($"DELETE [dbo].[tCNEWayBill] WHERE CNId={key}");
            var stockLogs = uow.RepositoryAsync<CNStockLog>().Queryable().Where(x => x.CNId == key);
            var slrepo = uow.RepositoryAsync<SalesLog>();
            var saleslog = await slrepo.Queryable().FirstOrDefaultAsync(x => x.CNId == key);
            if (saleslog != null)
            {
                if (saleslog.SalesVoucherId > 0)
                {
                    var vrepo = uow.RepositoryAsync<Voucher>();
                    var salesvoucher = await vrepo.FindAsync(saleslog.SalesVoucherId);
                    if (salesvoucher != null)
                    {
                        salesvoucher.ObjectState = ObjectState.Deleted;
                        vrepo.Delete(salesvoucher);
                    }
                }
                saleslog.ObjectState = ObjectState.Deleted;
                slrepo.Delete(saleslog);
            }
            if (await stockLogs.AnyAsync(x => x.Outwards.Sum(y => y.OutQty) > 0 && x.fk_Triplog.FormId != "1499"))
            {
                throw new BusinessException(ErrorCode.GLB106,
                    "Cannot Delete CN as it has been attached to TripLog/Challan");
            }

            foreach (var source in stockLogs.Include(x => x.fk_ChallanCN).ToList())
            {
                source.ObjectState = ObjectState.Deleted;
                if (source.fk_ChallanCN != null)
                {
                    source.fk_ChallanCN.ObjectState = ObjectState.Deleted;
                }
            }
            cnMaster.ObjectState = ObjectState.Deleted;
            _repo.Delete(cnMaster);
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction();
            }
            try
            {
                if (cnMaster.BillId > 0 && !cnMaster.CreateBillOnCNCreate)
                {
                    return BadRequest($"CN Cannot be deleted after creating Bill, CN No:{cnMaster.CNNo}");
                }
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tCNDTSStatusLog] WHERE CNId={key}");
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Commit();
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
                string navigationProperty, [FromBody] Uri link)
        {
            var cn = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (cn == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_Bill":
                    cn.fk_Bill = null;
                    cn.BillId = null;
                    break;

                case "fk_CnAdvanceId":
                    if (cn.CnAdvanceId.GetValueOrDefault(0) == 0) return StatusCode(HttpStatusCode.NoContent);
                    cn.fk_CnAdvanceId = null;
                    cn.CnAdvanceId = null;
                    break;

                case "fk_TripLog":
                    var chcnRepo = Request.GetContext()
                        .RepositoryAsync<CnChallan>();
                    var chcn = chcnRepo
                            .Queryable()
                            .FirstOrDefault(x => x.CNId == cn.Id && x.TriplogId == cn.TripLogId);
                    cn.fk_TripLog = null;
                    cn.TripLogId = null;
                    cn.TLLoadQty = 0;
                    if (chcn != null)
                    {
                        chcn.ObjectState = ObjectState.Deleted;
                        chcnRepo.Delete(chcn);
                    }

                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            try
            {
                cn.ObjectState = ObjectState.Modified;
                await _repo.UpdateAsync(cn);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/Consignments
        [HttpGet, EnableQuery]
        public IQueryable<CNMaster> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/Consignments(5)
        [EnableQuery]
        public SingleResult<CNMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PATCH: odata/Consignments(5)
        // PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNMaster> patch)
        {
            var uow = Request.GetContext();
            _repo.Request = Request;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNMaster cnMaster = await _repo.Queryable().Include(x => x.Materials).FirstOrDefaultAsync(x => x.Id == key);
            await uow.RepositoryAsync<CNStockLog>().Queryable().Where(x => x.CNId == key && x.RefStockId == null).Include(x => x.StockMMLogs).LoadAsync();
            if (cnMaster == null)
            {
                return NotFound();
            }
            patch.TryGetPropertyValue("JsonDataList", out var jsonDataList);
            var oldtriplogid = cnMaster.TripLogId;
            //var isdispatched =await uow.RepositoryAsync<CNStockLog>().Queryable().AnyAsync(x =>x.CNId==key&& x.LogTypeId != 1422 && x.fk_Triplog.FormId != "1499");

            //var challancnQuery = uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.CNId == key);
            //var challancount = await challancnQuery.CountAsync();
            //bool isdispatched = challancount > 0;
            //var challanviewid = await challancnQuery.OrderBy(x => x.ShipmentDate).Select(x => x.ViewId).FirstOrDefaultAsync();
            //var issamesource = challanviewid == cnMaster.ViewId;
            object actValue = cnMaster.ActualQty;
            object actWeight = cnMaster.ActualWeight;
            object mmvalues = cnMaster.MultiMaterialsView;

            patch.TryGetPropertyValue("ActualQty", out actValue);
            patch.TryGetPropertyValue("ActualWeight", out actWeight);
            patch.TryGetPropertyValue("MultiMaterialsView", out mmvalues);
            patch.TryGetPropertyValue("EWayBills", out var _ewaybills);
            var multimaterial = mmvalues as List<vwCNMultiMaterial>;
            var ewaybills= _ewaybills as List<vwEWayBill>;
            //( && challancount > 1)
            //if (isdispatched&&(!issamesource||challancount>1)&& ((multimaterial != null&&(multimaterial.Any(x=>x.IsDeleted)||cnMaster.Materials.Count!= multimaterial.Count)) || (decimal)actValue != cnMaster.ActualQty || (decimal)actWeight != cnMaster.ActualWeight))
            //{
            //    throw new BusinessException(ErrorCode.GLB106, "Consignment has been shipped so cannot be updated.");
            //}

            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                cnMaster.ObjectState = ObjectState.Modified;
                cnMaster.ConstCurTypeId = Helper.ConstCurTypeId;
                if (cnMaster.CNTypeId == 1369/*Cancelled*/)
                {
                    cnMaster.TripLogId = null;
                    cnMaster.TLLoadQty = 0;
                }
                //Request.GetHubContext().BroadCastMessageToSessionId(cnMaster.CreatedSessionId,$"CN No {cnMaster.CNNo} Created by you was updated by {Helper.GetLoggedInUserFullName()}.","CN Updated!!");
                patch.Patch(cnMaster);
                if (cnMaster.DeliveryTypeId.GetValueOrDefault(0)==0)
                {
                    cnMaster.DeliveryTypeId = cnMaster.ViewId != 1514/*SCM LR Master*/&&(cnMaster.TripLogId > 0 || (cnMaster.MultiMaterialsView?.Count ?? 0) == 0 || cnMaster.VehicleId.GetValueOrDefault()>0) ? 1472/*Direct Delivery*/ : 1545/*Stock Movement*/;
                }
                try
                {
                    var ewayrepo = uow.RepositoryAsync<CNEWayBill>();
                    if (ewaybills != null && ewaybills.Any())
                    {
                       
                        var existings = await ewayrepo.Queryable().Where(x => x.CNId == cnMaster.Id).Select(x => new { x.Id, x.EWayBillNo }).ToListAsync();
                        foreach (var ew in _mapper.Map<List<CNEWayBill>>(ewaybills))
                        {
                            ew.Id = existings?.FirstOrDefault(x => x.EWayBillNo == ew.EWayBillNo)?.Id ?? 0;
                            if (ew.Id > 0)
                            {
                                ewayrepo.Update(ew);
                            }
                            else
                            {
                                ewayrepo.Insert(ew);
                            }
                        }
                    }
                }
                catch
                {
                    //Ignore
                }
                await _repo.UpdateAsync(cnMaster);
                var tlid = cnMaster.TripLogId;
                if (jsonDataList is List<JsonDataEntity> dataview && dataview.Any())
                {
                    foreach (var entity in dataview)
                    {
                        cnMaster.DeleteAndAdd(entity);
                    }
                }
                await Request.GetContext().SaveChangesAsync();
                await uow.RepositoryAsync<VehicleMovementLog>().AttachCNToTripLogAsync(tlid, oldtriplogid, cnMaster.Id, cnMaster,true);
                //.AttachCNToTripLog(tlid, oldtriplogid, cnMaster);

                //await uow.SaveChangesAsync();
                try
                {
                    if (tlid > 0)
                    {
                        var triplog = await uow.RepositoryAsync<VehicleMovementLog>().Queryable().FirstOrDefaultAsync(x => x.Id == tlid);
                        var query = uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.TriplogId == cnMaster.TripLogId.Value);
                        var totalfreight = (await query.SumAsync(x => (decimal?)x.fk_CNMaster.CNSubTotalII)) ?? 0;
                        var totalqty = (await query.SumAsync(x => (decimal?)x.Qty)) ?? 0;
                        triplog.CNFreight = totalfreight;
                        triplog.LoadingQty = totalqty;
                        triplog.ObjectState = triplog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        //await uow.ExecSqlQueryAsync("UPDATE vml SET CNFreight=(SELECT SUM(cn.CNSubTotalII) FROM tCNChallan chn JOIN tCNMaster cn ON chn.CNId=cn.Id WHERE chn.TriplogId=vml.Id) FROM tVehicleMovementLog vml WHERE Id=@p0", new SqlParameter("p0", cnMaster.TripLogId.Value));
                    }
                }
                catch
                {
                    //Ignore
                }
                var repo = uow.RepositoryAsync<SalesLog>();
                var sl = await repo.Queryable().FirstOrDefaultAsync(x => x.CNId == cnMaster.Id) ?? new SalesLog();
                cnMaster.PrepareSalesLog(ref sl);
                switch (sl.ObjectState)
                {
                    case ObjectState.Added:
                        if (cnMaster.CNTotalFreight > 0) repo.Insert(sl);
                        break;

                    case ObjectState.Modified:
                        if (cnMaster.CNTotalFreight > 0) repo.Update(sl);
                        break;

                    case ObjectState.Deleted:
                        repo.Delete(sl);
                        break;
                }
                var salesConfig = uow.Context.GetApiConfig<int>("GenerateSalesVoucher");
                if (salesConfig == 1)
                {
                    var salesLedgers = await uow.Context.Ledgers.Where(x => x.Id == cnMaster.BillingPartyId).Select(x =>
                        new
                        {
                            x.SalesAccountId,
                            x.UnbilledSalesAcId
                        }).FromCacheFirstOrDefaultAsync();
                    if (salesLedgers.SalesAccountId == null || salesLedgers.UnbilledSalesAcId == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106, "Sales Account and Unabilled Sales Account are not Mapped to Billing Party.");
                    }

                    sl.SalesAccountId = salesLedgers.SalesAccountId;
                    sl.UnbilledSalesAcId = salesLedgers.UnbilledSalesAcId;
                }
                await uow.SaveChangesAsync();
                //if (tlid > 0)
                //{
                //    await uow.ExecSqlQueryAsync($"UPDATE T SET [CnFreight]=(SELECT SUM(CH.Revenue) FROM [dbo].[tCNCHallan] CH WHERE CH.TriplogId=T.Id) FROM [dbo].[tVehicleMovementLog] T WHERE T.Id={tlid}");
                //}
                if (_repo.GetConfigValue<int>("RunCNPostProcess") == 1)
                {
                    BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunCNPostProcess(null,cnMaster.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(2));
                }
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(cnMaster);
        }

        // POST: odata/Consignments
        public async Task<IHttpActionResult> Post(CNMaster cnMaster)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var ewaybills = cnMaster.EWayBills;
                if (cnMaster.AutoStationaryFieldId > 0 && (cnMaster.PageId.GetValueOrDefault()==0) && string.IsNullOrWhiteSpace(cnMaster.CNNo))
                {
                    var excludedtypes = new long[] { 1234, 1625 };
                    var newpageno = await uow.Context.Database.SqlQuery<StationaryResult>("EXEC [dbo].[Proc_GLB_GetStationary] @p0,@p1,@p2,@p3,@p4,@p5,@p6",
                        cnMaster.AutoStationaryFieldId, cnMaster.LoadingOfficeId, cnMaster.BillingPartyId, cnMaster.ViewId, 0, cnMaster.CNDate.Date, "")?.FirstOrDefaultAsync(x => !excludedtypes.Contains(x.NatureId));
                    if (newpageno != null)
                    {
                        cnMaster.CNNo = newpageno.PageNo;
                        cnMaster.PageId = newpageno.Id;
                    }
                    else
                    {
                        return BadRequest("Unable to Generate Consignment No. Check Stationery Availability !!!");
                    }
                }
                if (string.IsNullOrWhiteSpace(cnMaster.CNNo)) {
                    return BadRequest("CNNo Cannot be Saved Blank !!!");
                }

                //else
                //{
                //    var newpageno = await uow.Context.Database.SqlQuery<StationaryResult>("EXEC [dbo].[Proc_GLB_GetNewPageIfConsumed] @p0,@p1",cnMaster.PageId, cnMaster.CNNo)?.FirstOrDefaultAsync();
                //    if (newpageno != null)
                //    {
                //        cnMaster.CNNo = newpageno.PageNo;
                //        cnMaster.PageId = newpageno.Id;
                //    }
                //}

                cnMaster.ObjectState = ObjectState.Added;
                cnMaster.ConstCurTypeId = Helper.ConstCurTypeId;
                if (cnMaster.CNTypeId == 1369)
                {
                    cnMaster.TripLogId = null;
                    cnMaster.TLLoadQty = 0;
                }

                var topayconfig = _repo.GetConfigValue<int>("instancegenerationfortopay");
                var paidconfig = _repo.GetConfigValue<int>("instancegenerationforpaid");
                cnMaster.CreateBillOnCNCreate = (cnMaster.CNTypeId == 1366 && topayconfig == 1) || (cnMaster.CNTypeId == 1365 && paidconfig == 1);
                if (cnMaster.CreateBillOnCNCreate && cnMaster.CNTotalFreight <= 0)
                {
                    return BadRequest("CN Freight should be greater than zero while creating CN Bill");
                }
                var config =
                    new MapperConfiguration(cfg => cfg.CreateMap<vwCNMultiMaterial, CNMultiMaterial>())
                        .CreateMapper();
                var mmRepo = uow.RepositoryAsync<CNMultiMaterial>();
                foreach (var material in cnMaster.MultiMaterialsView)
                {
                    var entity = config.Map<CNMultiMaterial>(material);
                    entity.ObjectState = ObjectState.Added;
                    cnMaster.Materials.Add(entity);
                    entity.fk_CN = cnMaster;
                    entity.CnId = cnMaster.Id;
                    mmRepo.Insert(entity);
                }
                if (cnMaster.Materials.Any())
                {
                    //cnMaster.ActualQty =
                    //cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.ActualQty) ?? cnMaster.ActualQty;
                    //cnMaster.ActualWeight =
                    //cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.ActualWeight) ?? cnMaster.ActualWeight;
                    //cnMaster.ChargedQty =
                    //    cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.ChargeQty) ?? cnMaster.ChargedQty;
                    //cnMaster.ChargedWeight =
                    //    cnMaster.Materials?.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.ChargeWeight) ?? cnMaster.ChargedWeight;
                }

                if (cnMaster.ActualQty <= 0)
                {
                    cnMaster.ActualQty = cnMaster.ChargedQty > 0 ? cnMaster.ChargedQty : 1;
                }
                if (cnMaster.ChargedQty <= 0&&!cnMaster.IsZeroFreightCN)
                {
                    cnMaster.ChargedQty = cnMaster.ActualQty > 0 ? cnMaster.ActualQty : 1;
                }
                if (cnMaster.ActualWeight <= 0)
                {
                    cnMaster.ActualWeight = cnMaster.ChargedWeight > 0 ? cnMaster.ChargedWeight : 1;
                }
                if (cnMaster.ChargedWeight <= 0 && !cnMaster.IsZeroFreightCN)
                {
                    cnMaster.ChargedWeight = cnMaster.ActualWeight > 0 ? cnMaster.ActualWeight : 1;
                }
                var _IsConsignmentWithOffLoadingPoint = _repo.GetConfigValue<int>("IsConsignmentWithOffLoadingPoint");
                if (_IsConsignmentWithOffLoadingPoint == 1 && (cnMaster.ViewId==1514 || cnMaster.ViewId == 1571))
                {
                    /*only in case of fuel business*/
                    var rt = await uow.RepositoryAsync<RouteMaster>().Queryable().FirstOrDefaultAsync(x => x.Id == cnMaster.ActualRouteId);
                    cnMaster.LoadingPointId = (cnMaster.LoadingPointId == rt.FromPlaceId) ? cnMaster.LoadingPointId : rt.FromPlaceId;
                }
                _repo.Insert(cnMaster);
                if (cnMaster.DeliveryTypeId.GetValueOrDefault(0) == 0)
                {
                    cnMaster.DeliveryTypeId = cnMaster.ViewId != 1514/*SCM LR Master*/&& (cnMaster.TripLogId > 0 || (cnMaster.MultiMaterialsView?.Count ?? 0) == 0 || cnMaster.VehicleId.GetValueOrDefault() > 0) ? 1472/*Direct Delivery*/ : 1545/*Stock Movement*/;
                }
                await uow.SaveChangesAsync();
                try
                {
                    
                    if (ewaybills != null && ewaybills.Any())
                    {
                        var ewayrepo = uow.RepositoryAsync<CNEWayBill>();
                        var existings = await ewayrepo.Queryable().Where(x => x.CNId == cnMaster.Id).Select(x => new { x.Id, x.EWayBillNo }).ToListAsync();
                        foreach (var ew in _mapper.Map<List<CNEWayBill>>(ewaybills))
                        {
                            ew.Id = existings?.FirstOrDefault(x => x.EWayBillNo == ew.EWayBillNo)?.Id ?? 0;
                            if (ew.Id > 0)
                            {
                                ewayrepo.Update(ew);
                            }
                            else
                            {
                                ewayrepo.Insert(ew);
                            }
                        }
                    }
                }
                catch
                {
                    //Ignore
                }
                if (cnMaster.TripLogId.HasValue && !await uow.RepositoryAsync<CNStockLog>().Queryable().AnyAsync(x => x.TriplogId == cnMaster.TripLogId && x.CNId == cnMaster.Id))
                {
                    await uow.RepositoryAsync<VehicleMovementLog>().AttachCNToTripLogAsync(cnMaster.TripLogId.Value, cnMaster.TripLogId.Value, cnMaster.Id,cnMaster);
                    await uow.SaveChangesAsync();
                    try
                    {
                        if (cnMaster.TripLogId > 0)
                        {
                            var triplog = await uow.RepositoryAsync<VehicleMovementLog>().Queryable().FirstOrDefaultAsync(x => x.Id == cnMaster.TripLogId);
                            var query = uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.TriplogId == cnMaster.TripLogId.Value);
                            var totalfreight = (await query.SumAsync(x => (decimal?)x.fk_CNMaster.CNSubTotalII)) ?? 0;
                            var totalqty = (await query.SumAsync(x => (decimal?)x.Qty)) ?? 0;
                            triplog.CNFreight = totalfreight;
                            triplog.LoadingQty = totalqty;
                            triplog.ObjectState = triplog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                            
                            //await uow.ExecSqlQueryAsync("UPDATE vml SET CNFreight=(SELECT SUM(cn.CNSubTotalII) FROM tCNChallan chn JOIN tCNMaster cn ON chn.CNId=cn.Id WHERE chn.TriplogId=vml.Id) FROM tVehicleMovementLog vml WHERE Id=@p0", new SqlParameter("p0", cnMaster.TripLogId.Value));
                        }

                    }
                    catch
                    {
                        //Ignore
                    }
                    
                }
                //var sl = new SalesLog();
                //cnMaster.PrepareSalesLog(ref sl);
                //uow.RepositoryAsync<SalesLog>().Insert(sl);
                //var salesConfig = uow.Context.GetApiConfig<int>("GenerateSalesVoucher");
                //if (salesConfig == 1)
                //{
                //    var salesLedgers = await uow.Context.Ledgers.Where(x => x.Id == cnMaster.BillingPartyId).Select(x =>
                //        new
                //        {
                //            x.SalesAccountId,
                //            x.UnbilledSalesAcId
                //        }).FromCacheFirstOrDefaultAsync();
                //    if (salesLedgers.SalesAccountId == null || salesLedgers.UnbilledSalesAcId == null)
                //    {
                //        throw new BusinessException(ErrorCode.GLB106, "Sales Account and Unabilled Sales Account are not Mapped to Billing Party.");
                //    }

                //    sl.SalesAccountId = salesLedgers.SalesAccountId;
                //    sl.UnbilledSalesAcId = salesLedgers.UnbilledSalesAcId;
                //}
                await uow.SaveChangesAsync();
                if (_repo.GetConfigValue<int>("RunCNPostProcess") == 1)
                {
                    BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunCNPostProcess(null,cnMaster.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(2));
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(cnMaster);
        }

        // POST: odata/Consignments(key)/Materials
        [ODataRoute("Consignments({key})/Materials")]
        public async Task<IHttpActionResult> PostMaterials([FromODataUri]long key, [FromBody] CNMultiMaterial material)
        {
            if (!_repo.Queryable().Any(x => x.Id == key))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            material.CnId = key;
            material.ObjectState = ObjectState.Added;
            var item = _CnMMrepo.Insert(material);
            await uow.SaveChangesAsync();
            return Created(item);
        }

        // PUT: odata/Consignments(5)
        public async Task<IHttpActionResult> Put(long key, CNMaster cnMaster)
        {
            return StatusCode(HttpStatusCode.Forbidden);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != cnMaster.Id)
            {
                return BadRequest();
            }

            cnMaster.ObjectState = ObjectState.Modified;
            cnMaster.ConstCurTypeId = Helper.ConstCurTypeId;
            _repo.Update(cnMaster);
            await Request.GetContext().SaveChangesAsync();

            return Updated(cnMaster);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}