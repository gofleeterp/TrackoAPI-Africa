using Microsoft.TeamFoundation.SourceControl.WebApi.Legacy;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

//using HibernatingRhinos.Profiler.Appender.ProfiledDataAccess;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class BulkAdvanceInsertController : ODataController
    {
        private readonly ITripAdvanceLogService _tla;

        public BulkAdvanceInsertController(ITripAdvanceLogService tla)
        {
            _tla = tla;
        }

        [HttpPost]
        public async Task<IHttpActionResult> BatchPostAdvances(ODataActionParameters parameters)
        {
            var ivouchers = parameters["vouchers"] as IEnumerator<vwAdvanceVoucher>;
            if (ivouchers == null) return BadRequest("No Records found to upload");
            var vouchers = ivouchers.ToList();
            var uow = Request.GetContext();
            _tla.Request = this.Request;
            var transaction = uow.Context.Database.CurrentTransaction ??
                                  uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                //#if !DEBUG
                await _tla.BatchInsert(vouchers, transaction.UnderlyingTransaction);
                //#elif DEBUG
                //await _tla.BatchInsert(vouchers, transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction);
                //#endif

                
                string batchids = vouchers.Select(x => x.BatchId).Aggregate(string.Empty, (current, batchid) => current + ((string.IsNullOrWhiteSpace(current) ? "" : "^") + batchid));
                var item = new vwBatch { BatchId = batchids, BatchSize = vouchers.Count };
                var spname = await uow.RepositoryAsync<ReportProcedure>().FindAsync(540);
                if (spname != null)
                {
                    try
                    {
                        await uow.ExecuteProcedureAsync(spname.StoredProcedureName, new SqlParameter("TransactionId", -1), new SqlParameter("TransactionNumber", batchids), new SqlParameter("TransactionType", 1107));
                    }
                    catch (SqlException ex)
                    {
                        throw new BusinessException(ex);
                    }
                }
                if (!Request.IsBatchRequest())
                {
                    transaction.Commit();
                    transaction.Dispose();
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
                throw;
            }
        }

        [HttpPost, ODataRoute("BulkAdvanceWithVoucher")]
        public async Task<IHttpActionResult> BulkAdvanceWithVoucher(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var advances = (parameters["advances"] as IEnumerator<TripAdvanceLog>)?.ToList();
            if (advances == null) return BadRequest("Advances Parameter is Null");
            var voucherstring = parameters["voucher"]?.ToString();
            if (string.IsNullOrWhiteSpace(voucherstring)) return BadRequest("Voucher Parameter is Null");
            var voucherraw = JsonConvert.DeserializeObject<Voucher>(voucherstring);
            var voucher = new Voucher
            {
                Id = voucherraw.Id
            };
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                //await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE ISNULL(SettlementId,0)=0 AND VoucherId IS NOT NULL AND VoucherId={(voucher?.Id ?? 0)}");
                var vds = voucherraw.VoucherDetails;
                vds?.ForEach(x => x.Voucher = null);
                voucherraw.VoucherDetails = new List<VoucherDetail>();
                var vrepo = uow.RepositoryAsync<Voucher>();

                if (voucher.Id > 0)
                {
                    await uow.ExecSqlQueryAsync($"DELETE FROM [dbo].[tVoucherVD] WHERE VoucherId={(voucher?.Id ?? 0)}");
                    var rowversion = await vrepo.Queryable().Where(x => x.Id == voucher.Id).Select(x => x.RowVersion).FirstOrDefaultAsync();
                    voucher.RowVersion = voucherraw.RowVersion = rowversion;
                    vrepo.Update(voucher);
                    AutoMapper.Mapper.Map(voucherraw, voucher);
                    voucher.ObjectState = ObjectState.Modified;
                    voucher.RowVersion = rowversion;
                }
                else
                {
                    voucher.ObjectState = ObjectState.Added;
                    AutoMapper.Mapper.Map(voucherraw, voucher);
                    voucher = vrepo.Insert(voucher);
                    voucher.ObjectState = ObjectState.Added;
                }
                voucher.ConstCurTypeId = Helper.ConstCurTypeId;
                voucher.IsCCRequired = true;
                await uow.SaveChangesAsync();
                if (voucher.VoucherDetails == null || !voucher.VoucherDetails.Any())
                {
                    voucher.VoucherDetails = vds;
                }
                voucher?.VoucherDetails?.ForEach(vd =>
                {
                    vd.Id = 0;
                    if (voucher.Id > 0)
                    {
                        vd.VoucherId = voucher.Id;
                        vd.ConstCurTypeId = voucher.ConstCurTypeId;
                        vd.CurTypeId = voucher.CurTypeId;
                        vd.CurRate = voucher.CurRate;
                        vd.IsCCRequired = voucher.IsCCRequired;
                    }
                    vd.Voucher = voucher;
                    uow.RepositoryAsync<VoucherDetail>().Insert(vd);
                    vd.ObjectState = ObjectState.Added;
                    vd.VoucherDetailReferences?.ForEach(vdr =>
                    {
                        vdr.Id = 0;
                        vdr.fk_VoucherDetail = vd;
                        uow.RepositoryAsync<VoucherDetailReference>().Insert(vdr);
                        vdr.ObjectState = ObjectState.Added;

                        vdr.ConstCurTypeId = vd.ConstCurTypeId;
                        vdr.CurTypeId = vd.CurTypeId;
                        vdr.CurRate = vd.CurRate;
                        vdr.IsCCRequired = voucher.IsCCRequired;
                    });
                });
                await uow.SaveChangesAsync();
                var idsx = advances.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                var advs = await _tla.Queryable().Where(x => idsx.Contains(x.Id)).Select(x => new { x.Id, x.RowVersion }).ToListAsync();
                advances?.ForEach(ad =>
                {
                    if (ad.Id > 0)
                    {
                        ad.RowVersion = advs.FirstOrDefault(x => x.Id == ad.Id)?.RowVersion;
                    }
                    ad.ConstCurTypeId = voucher.ConstCurTypeId;
                    ad.CurTypeId = voucher.CurTypeId;
                    ad.CurRate = voucher.CurRate;
                    ad.VoucherId = voucher.Id;
                    ad.fk_Voucher = voucher;
                    ad.ObjectState = ad.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    if (ad.Id > 0)
                    {
                        _tla.Update(ad);
                    }
                    else
                    {
                        _tla.Insert(ad);
                    }
                });
                var ids = advances.Where(x => x.Id > 0).Select(x => x.Id);
                var deletedRecords = await (from a in _tla.Queryable().Where(x => x.VoucherId == voucher.Id)
                                            where !ids.Contains(a.Id)
                                            select a).ToListAsync();
                foreach (var x in deletedRecords)
                {
                    if (x.SettlementId.HasValue) throw new BusinessException(ErrorCode.TADV105, $"Reference No {x.ReferenceNo} is settled");
                    x.ObjectState = ObjectState.Deleted;
                    x.VoucherId = null;
                    x.fk_Voucher = null;
                    _tla.Delete(x);
                }
                await uow.SaveChangesAsync();
                if (parameters["procid"] is long procid && procid > 0)
                {
                    var spname = await uow.RepositoryAsync<ReportProcedure>().FindAsync(procid);
                    if (spname != null)
                    {
                        try
                        {
                            await uow.ExecuteProcedureAsync(spname.StoredProcedureName, new SqlParameter("TransactionId", voucher.Id), new SqlParameter("TransactionNumber", voucher.VoucherNo), new SqlParameter("TransactionType", voucher.ViewId));
                        }
                        catch (SqlException ex)
                        {
                            throw new BusinessException(ex);
                        }
                    }
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
            return Ok(voucher.Id);
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            try
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
                }
                var voucher = Request.GetContext().RepositoryAsync<Voucher>().Query(x => x.Id == key).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (voucher == null)
                {
                    return NotFound();
                }
                _tla.BulkDelete(voucher);
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
        }

        // GET: odata/BulkAdvanceInserts(5)
        [EnableQuery]
        public SingleResult<vwAdvanceVoucher> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_tla.GetQueryableBulkEntryByKey(key));
        }

        //// PATCH: odata/TripAdvanceLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [System.Web.Http.AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<vwAdvanceVoucher> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            vwAdvanceVoucher advance;
            try
            {
                var uow = Request.GetContext();
                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                advance = _tla.GetBulkEntryByKey(key);
                if (advance == null)
                {
                    return NotFound();
                }
                patch.Patch(advance);
                var settledacid = uow.Context.GetApiConfig<long>("DefaultSettledAccountId");

                var refflagoffaccids = await uow.RepositoryAsync<Ledger>()
                    .Queryable()
                    .Where(x => x.Id == settledacid && !x.ReferenceFlag).Select(x => x.AccountName).FirstOrDefaultAsync();
                if (!string.IsNullOrWhiteSpace(refflagoffaccids)) return BadRequest("Advance Control Account Require Bill Reference Flag ON");
                //if (advance.TripAdvanceLogs.Any(x => x.Amount <= 0))
                //    return BadRequest("One of Advance Amount is Zero which is not allowed.");
                var voucher = uow.RepositoryAsync<Voucher>().Query(x => x.Id == key && x.VoucherTypeId == advance.AdvanceTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE ISNULL(SettlementId,0)=0 AND VoucherId={(voucher?.Id ?? 0)}");

                advance.ConstCurTypeId = Helper.ConstCurTypeId;
                var vch = _tla.BulkAdvance(advance, voucher);
                await uow.SaveChangesAsync();
                advance.Id = vch.Id;
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }

            return Updated(new vwAdvanceVoucher() { Id = advance.Id });
        }

        // POST: odata/TripAdvanceLogs
        public async Task<IHttpActionResult> Post(vwAdvanceVoucher adv)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //if (adv.TripAdvanceLogs.Any(x => x.Amount <= 0))
            //    return BadRequest("One of Advance Amount is Zero which is not allowed.");
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var voucher = uow.RepositoryAsync<Voucher>().Query(x => x.Id == adv.Id && x.VoucherTypeId == adv.AdvanceTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE ISNULL(SettlementId,0)=0 AND VoucherId={(voucher?.Id ?? 0)}");
            adv.ConstCurTypeId = Helper.ConstCurTypeId;
            var vch = _tla.BulkAdvance(adv, voucher);
            try
            {
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
            adv.Id = vch.Id;
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(adv);
        }

        // PUT: odata/TripAdvanceLogs(5)
        public async Task<IHttpActionResult> Put(long key, vwAdvanceVoucher adv)
        {
            var uow = Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != adv.Id)
            {
                return BadRequest();
            }
            //if (adv.TripAdvanceLogs.Any(x => x.Amount == 0))
            //    return BadRequest("One of Advance Amount is Zero which is not allowed.");
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var voucher = uow.RepositoryAsync<Voucher>().Query(x => x.Id == key && x.VoucherTypeId == adv.AdvanceTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE ISNULL(SettlementId,0)=0 AND VoucherId={(voucher?.Id ?? 0)}");
            adv.ConstCurTypeId = Helper.ConstCurTypeId;
            _tla.BulkAdvance(adv, voucher);

            try
            {
                await uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Ok();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}