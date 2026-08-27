using AutoMapper;
using Newtonsoft.Json;
using Repository.Pattern.Core.UnitOfWork;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.vw.ts;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNBillsController : ODataController
    {
        private readonly ICNBillService _repo;
        private ICNBillLogService _logRepo;
        private readonly IMapper _mapper;
        private readonly bool IsSmartInvoicingActivated;
        public CNBillsController(ICNBillService service,ICNBillLogService billLogService, IMapper mapper)
        {
            _repo =  service;
            _logRepo = billLogService;
            _mapper = mapper;
            IsSmartInvoicingActivated = _repo.GetConfigValue<int>("IsSmartInvoicingActivated") == 1;
        }
        // GET: odata/CNBillMaster
        [HttpGet, EnableQuery]
        public IQueryable<CNBill> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNBillMaster(5)
        [EnableQuery]
        public SingleResult<CNBill> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/CNBillMaster(5)
        public async Task<IHttpActionResult> Put(long key, CNBill CNBill)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != CNBill.Id)
            {
                return BadRequest();
            }
            var uow = Request.GetContext();
            
            if (IsSmartInvoicingActivated)
            {
                var err = Get_TPTPAY_LiveDbLevelValidation(CNBill, uow,"update");
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
            }

            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {

                CNBill.ConstCurTypeId = Helper.ConstCurTypeId;
                CNBill.ObjectState = ObjectState.Modified;
                var blrepo = uow.RepositoryAsync<CNBillLog>();
                _repo.Update(CNBill);

                try
                {
                    if (CNBill.Id > 0)
                    {
                        /*Deleting Dedn MR PL Logs*/
                        _repo.ExecuteSql($"DELETE pl FROM dbo.tCNBillPaymentLog as pl join dbo.tCNBillPayment as p on pl.PaymentId=p.Id WHERE p.VoucherTypeId=131 and pl.BillId={CNBill.Id}");
                    }
                }
                catch (SqlException e)
                {
                    throw new BusinessException(e);
                }

                CNBill.BillLogs?.ForEach(x =>
                {
                    switch (x.ObjectState)
                    {
                        case ObjectState.Added:
                            blrepo.Insert(x);
                            break;
                        case ObjectState.Modified:
                            blrepo.Update(x);
                            break;
                    }
                });
                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return Updated(CNBill);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
        }
        // POST: odata/CNBillMaster
        public async Task<IHttpActionResult> Post(CNBill CNBill)
        {
            CNBill.ObjectState = ObjectState.Added;
            CNBill.ConstCurTypeId = Helper.ConstCurTypeId;

            var _jsonBillLogs = CNBill.JsonBillLogs;
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var ch = _repo.Insert(CNBill);

                await uow.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(_jsonBillLogs) && (ch.BillLogs?.Count ?? 0) == 0)
                {
                    var blrepo = uow.RepositoryAsync<CNBillLog>();
                    ch.BillLogs = JsonConvert.DeserializeObject<List<CNBillLog>>(_jsonBillLogs);
                    ch.BillLogs?.ForEach(x =>
                    {
                        if (x.Id == 0)
                        {
                            x.ObjectState = ObjectState.Added;
                            x.BillId = ch.Id;
                            x.fk_Bill = ch;
                            blrepo.Insert(x);
                        }
                        else
                        {
                            x.ObjectState = ObjectState.Modified;
                            x.BillId = ch.Id;
                            x.fk_Bill = ch;
                            blrepo.Update(x);
                        }
                    });
                    await uow.SaveChangesAsync();


                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return Created(ch);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
        }
        //// PATCH: odata/CNBillMaster(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNBill> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            CNBill ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            
            var voucherid = ch.VoucherId;
            var _jsonBillLogs = patch.GetEntity().JsonBillLogs;
            patch.Patch(ch);

            if (IsSmartInvoicingActivated)
            {
                var err = Get_TPTPAY_LiveDbLevelValidation(ch, uow,"update");
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
            }

            ch.ConstCurTypeId = Helper.ConstCurTypeId;
            ch.ObjectState = ObjectState.Modified;
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                
                try
                {
                    if (ch.Id > 0)
                    {
                        /*Deleting Dedn MR PL Logs*/
                        _repo.ExecuteSql($"DELETE pl FROM dbo.tCNBillPaymentLog as pl join dbo.tCNBillPayment as p on pl.PaymentId=p.Id WHERE p.VoucherTypeId=131 and pl.BillId={ch.Id}");
                    }
                }
                catch (SqlException e)
                {
                    throw new BusinessException(e);
                }

                await uow.SaveChangesAsync();                
                if (!string.IsNullOrWhiteSpace(_jsonBillLogs) && (ch.BillLogs?.Count ?? 0) == 0)
                {
                    var blrepo = uow.RepositoryAsync<CNBillLog>();
                    var billlogs = JsonConvert.DeserializeObject<List<CNBillLog>>(_jsonBillLogs);
                    billlogs?.ForEach(x =>
                    {
                        if (x.Id == 0)
                        {
                            x.ObjectState = ObjectState.Added;
                            x.BillId = ch.Id;
                            x.fk_Bill = ch;
                            blrepo.Insert(x);
                        }
                        else
                        {
                            var bl = new CNBillLog { Id = x.Id };
                            blrepo.Update(bl);
                            _mapper.Map(x, bl);
                            bl.ObjectState = ObjectState.Modified;
                            bl.BillId = ch.Id;
                            bl.fk_Bill = ch;
                            bl.TotalBillAmount = x.TotalBillAmount;
                        }

                    });
                    await uow.SaveChangesAsync();
                }
                var bd = await uow.SaveChangesAsync();
                if (voucherid.GetValueOrDefault() > 0 && bd > 0&&ch.VoucherId.GetValueOrDefault()==0)
                {
                    _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={voucherid}");
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
            return Updated(ch);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var bill = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (bill == null)
            {
                return NotFound();
            }
            try
            {
                switch (navigationProperty)
                {
                    case "fk_Voucher":
                        long? voucherid = 0;
                        voucherid = bill.VoucherId;
                        bill.fk_Voucher = null;
                        bill.VoucherId = null;
                        var bd = await Request.GetContext().SaveChangesAsync();
                        if (voucherid.GetValueOrDefault() > 0 && bd > 0)
                        {
                            _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={voucherid}");
                        }
                        
                        break;
                    case "fk_VDR":
                        bill.fk_VDR = null;
                        bill.VDRId = null;
                        bill.ObjectState=ObjectState.Modified;
                        await Request.GetContext().SaveChangesAsync();
                        break;
                    case "fk_CoverNote":
                        bill.fk_CoverNote = null;
                        bill.CoverNoteId = null;
                        bill.ObjectState = ObjectState.Modified;
                        _repo.RevokeBillSubmission(bill);
                        await Request.GetContext().SaveChangesAsync();
                        break;
                    default:
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
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
        // DELETE: odata/CNBillMaster(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            var cnBill = await _repo.FindAsync(key);
            if (cnBill == null)
            {
                return NotFound();
            }
            try
            {
                if (IsSmartInvoicingActivated)
                {
                    var err = Get_TPTPAY_LiveDbLevelValidation(cnBill, uow, "delete");
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        return BadRequest(err);
                    }
                }

                try
                {
                    _repo.ExecuteSql($"EXEC [dbo].[Proc_TRANS_1480_BillDelete] @TransactionId={cnBill.Id},@TransactionNumber='{cnBill.BillNo}',@TransactionType=67,@JsonData='[]'");
                }
                catch (SqlException e)
                {
                    throw new BusinessException(e);
                }

                cnBill.ObjectState = ObjectState.Deleted;
                _repo.Delete(cnBill);
                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                var vchid = cnBill.VoucherId;

                var bd = await uow.SaveChangesAsync();
                if (vchid.GetValueOrDefault() > 0 && bd > 0)
                {
                    _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={vchid}");
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
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
            return StatusCode(HttpStatusCode.NoContent);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }


        // POST: odata/CNBills(key)/BillLogs
        [AcceptVerbs("POST")]
        [ODataRoute("CNBills({key})/BillLogs")]
        public async Task<IHttpActionResult> PostCnBillLogs([FromODataUri]long key, [FromBody] CNBillLog billlog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var billExists = await _repo.Queryable().Include(x=>x.fk_BillNature).Select(x=>new { x.Id,x.fk_BillNature.CNBillTypeId}).FirstOrDefaultAsync(x=>x.Id==key);
            if (billExists==null)
            {
                return NotFound();
            }
            if (billExists.CNBillTypeId == 1363)
            {
                //var cn =await uow.RepositoryAsync<CNMaster>().FindAsync(billlog.CNId);
                //cn.BillId = key;
                //cn.ObjectState=ObjectState.Modified;
                _repo.ExecuteSql($"UPDATE [dbo].[tCNMaster] SET [BillId] = {key} WHERE  [Id] = {billlog.CNId.GetValueOrDefault()}");
            }

            billlog.BillId = key;
            billlog.ObjectState = ObjectState.Added;

            try
            {
                _logRepo.Insert(billlog);
                await uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (billlog.CNId>0&&await CnBillLogExists(billlog.CNId.GetValueOrDefault(0)))
                {
                    return BadRequest("CN Already Mapped to this Bill");
                }
            }

            return Created(billlog);
        }
        private async Task<bool> CnBillLogExists(long cnid)
        {
            return await Request.GetContext().RepositoryAsync<CNBillLog>().Queryable().AnyAsync(e => e.CNId == cnid);
        }
        //[HttpPost]
        //public async Task<IHttpActionResult> ArchiveBill([FromODataUri] long key)
        //{
        //    var uow = Request.GetContext();
        //    if (!Request.IsBatchRequest())
        //    {
        //        uow.BeginTransaction();
        //    }
        //    try
        //    {
        //        if (!ModelState.IsValid || key <= 0)
        //        {
        //            return BadRequest();
        //        }
        //        if (await _logRepo.Queryable().AnyAsync(x => x.BillId == key && x.PaymentLogs.Count() > 0))
        //        {
        //            return BadRequest("Bill for those payment has been received cannot be archived");
        //        }
        //        var archives = await _logRepo.Queryable().Where(x => x.BillId == key)
        //            .AsNoTracking()
        //            .Select(x => new CNBillLogArchive
        //            {
        //                AOther1Amount = x.AOther1Amount,
        //                BillId = x.BillId,
        //                AOther2Amount = x.AOther2Amount,
        //                AOther3Amount = x.AOther3Amount,
        //                AOther4Amount = x.AOther4Amount,
        //                AOther5Amount = x.AOther5Amount,
        //                AOther6Amount = x.AOther6Amount,
        //                AutoStationaryFieldId = x.AutoStationaryFieldId,
        //                BalanceAmount = x.BalanceAmount,
        //                BillingPartyAccountId = x.BillingPartyAccountId,
        //                CGSTACId = x.CGSTACId,
        //                CGSTAmount = x.CGSTAmount,
        //                CGSTRate = x.CGSTRate,
        //                CNFreight = x.CNFreight,
        //                CNId = x.CNId,
        //                CNNo = x.fk_CN != null ? x.fk_CN.CNNo : null,
        //                DiscountAmount = x.DiscountAmount,
        //                DiscountRate = x.DiscountRate,
        //                FreightCalcCriteria = x.FreightCalcCriteria,
        //                HSNCodeId = x.HSNCodeId,
        //                Id = x.Id,
        //                IGSTACId = x.IGSTACId,
        //                IGSTAmount = x.IGSTAmount,
        //                IGSTRate = x.IGSTRate,
        //                ISTAmount = x.ISTAmount,
        //                LOther1Amount = x.LOther1Amount,
        //                LOther2Amount = x.LOther2Amount,
        //                LOther3Amount = x.LOther3Amount,
        //                LOther4Amount = x.LOther4Amount,
        //                NewRate = x.NewRate,
        //                NonTaxableAmount = x.NonTaxableAmount,
        //                ObjectState = ObjectState.Added,
        //                OldRate = x.OldRate,
        //                ParticularId = x.ParticularId,
        //                RoundOff = x.RoundOff,
        //                SalesLogId = x.SalesLogId,
        //                SGSTACId = x.SGSTACId,
        //                SGSTAmount = x.SGSTAmount,
        //                SGSTRate = x.SGSTRate,
        //                Subtotal1 = x.Subtotal1,
        //                Subtotal2 = x.Subtotal2,
        //                SubTotal3 = x.SubTotal3,
        //                TripLogId = x.TripLogId,
        //                TotalBillAmount = x.TotalBillAmount,
        //                TripLogNo = x.fk_TripLog != null ? x.fk_TripLog.TriplogNo : null,
        //                UserRemark = x.UserRemark
        //            }).ToListAsync();

        //        if (!Request.IsBatchRequest())
        //        {
        //            uow.Commit();
        //        }
        //        return StatusCode(HttpStatusCode.NoContent);
        //    }catch
        //    {
        //        if (!Request.IsBatchRequest())
        //        {
        //            uow.Rollback();
        //        }
        //    }
        //}
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var bill = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (bill==null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var id = Request.GetKeyFromUri<long>(link);
                switch (navigationProperty)
                {
                    case "fk_Voucher":
                        var voucher =
                            await
                                uow.RepositoryAsync<Voucher>().Queryable().AnyAsync(x => x.Id == id);
                        if (!voucher)
                        {
                            if (!Request.IsBatchRequest())
                            {
                                uow.Rollback();
                            }
                            return NotFound();
                        }
                        //bill.VoucherId = id;
                        var result = _repo.ExecuteSql($"UPDATE [dbo].[tCNBillMaster] SET [VoucherId]={id} WHERE Id={key}");
                        if (result <= 0)
                        {
                            if (!Request.IsBatchRequest())
                            {
                                uow.Rollback();
                            }
                            return BadRequest("Invalid Voucher for Bill");
                        }
                        break;
                    case "fk_VDR":

                        var vdrepo = uow.RepositoryAsync<VoucherDetailReference>();
                        var vdr = await
                            vdrepo.Queryable().FirstOrDefaultAsync(x => x.Id == id);
                        if (vdr == null)
                        {
                            if (!Request.IsBatchRequest())
                            {
                                uow.Rollback();
                            }
                            return NotFound();
                        }
                        bill.VDRId = id;
                        vdr.TransactionId = key;
                        vdr.ObjectState = ObjectState.Modified;
                        bill.ObjectState = ObjectState.Modified;
                        await uow.SaveChangesAsync();
                        break;
                    case "fk_CoverNote":
                        var billsubRepo = uow.RepositoryAsync<BillSubmission>();
                        var bs = await
                            billsubRepo.Queryable().FirstOrDefaultAsync(x => x.Id == id);
                        if (bs == null)
                        {
                            if (!Request.IsBatchRequest())
                            {
                                uow.Rollback();
                            }
                            return NotFound();
                        }
                        if (bill.BillSubDate == null)
                        {
                            bill.BillSubDate = bs.DocDate;
                        }
                        bill.fk_CoverNote = bs;
                        bill.CoverNoteId = bs.Id;
                        bill.ObjectState = ObjectState.Modified;
                        _repo.ApplyBillSubmission(bill);
                         await Request.GetContext().SaveChangesAsync();
                        break;
                    default:
                        if (!Request.IsBatchRequest())
                        {
                            uow.Rollback();
                        }
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            
        }

        [HttpGet, EnableQuery]
        public IQueryable<CNBill> GetPendingBills(ODataQueryOptions options)
        {
            var query= _repo.Queryable().Where(x => x.BillLogs.Sum(y => y.BalanceAmount) > 0);
            options.ApplyTo(query);
            return query;
        }
        [HttpPost]
        public IHttpActionResult VerifyDataIntegration(ODataActionParameters parameters)
        {
            var obj = parameters["Ids"] as string;
            if (string.IsNullOrWhiteSpace(obj))
            {
                return BadRequest("Unable to Verify data integrity");
            }
            var ids = obj.Split(';').Select(x => new
            {
                BillId=x.Split('-')[0],
                VoucherId= x.Split('-')[1]
            });
            var bills = string.Empty;
            foreach (var id in ids)
            {
                
            }
            return Ok("");
        }
        private string Get_TPTPAY_LiveDbLevelValidation(CNBill _record, IUnitOfWorkAsync _uow,string _event)
        {
            var obj = new
            {
                _record.BillingPartyAccountId,
                _record.VoucherId,
                _record.CurTypeId,
                _record.CurRate
            };

            var livevalidationerr = _uow.SqlQueryAsync(
            "[dbo].[dbo].[Proc_GBL_TPTPAY_LiveValidationV1]",
            new SqlParameter() { Value = _record.Id, ParameterName = "parameter1" }/*Id*/,
            new SqlParameter() { Value = _record.TPT_RequestId, ParameterName = "parameter2" }/*requestId*/,
            new SqlParameter() { Value = _record.ViewId, ParameterName = "parameter3" }/*Viewid*/,
            new SqlParameter() { Value = _event, ParameterName = "parameter4" }/*event, create,update,delete*/,
            new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter5" }/*SessionId*/,
            new SqlParameter() { Value = JsonConvert.SerializeObject(obj), ParameterName = "parameter11" }/*model*/
            ).Result;

            if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
            {
                return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
            }
            return "";
        }
    }
}