using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TrackoAPI.ViewModels.Global;
using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.SourceControl.WebApi.Legacy;
using TrackoAPI.ViewModels.AMS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlClient;
using System.Data;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VouchersController : ODataController
    //ODataController
    {
        private readonly IVoucherService _repo;

        public VouchersController(IVoucherService service)
        {
            _repo = service;
        }
        // GET: odata/Vouchers
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<Voucher> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/Vouchers(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<Voucher> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Vouchers(5)
        public async Task<IHttpActionResult> Put(long key, Voucher objVoucher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVoucher.Id)
            {
                return BadRequest();
            }


            objVoucher.ObjectState = ObjectState.Modified;
            _repo.Update(objVoucher);

            try
            {
                await Request.GetContext().SaveChangesAsync();

                try
                {
                    if (objVoucher.VdrJson.Any())
                    {
                        _repo.ExecuteSql($"exec [Proc_GBL_GroupVoucher_ApiCalls] @TransactionId={objVoucher.Id},@TransactionNumber='{objVoucher.VoucherNo}',@TransactionType={objVoucher.ViewId.GetValueOrDefault()},@ActionType=1,@JsonData='{JsonConvert.SerializeObject(objVoucher.VdrJson)}'");
                    }
                }
                catch (SqlException ex)
                {
                    throw new BusinessException(ex.Message);
                }


                if (objVoucher.RemoveVD)
                {
                    await _repo.ExecuteSqlAsync($"DELETE FROM [dbo].[tVoucherVD] where VoucherId={objVoucher.Id}");
                }

            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            return Updated(objVoucher);
        }
        // POST: odata/Vouchers
        public async Task<IHttpActionResult> Post(Voucher objVoucher)
        {
            objVoucher.ObjectState = ObjectState.Added;
            objVoucher.JsonData = JsonConvert.SerializeObject(objVoucher.Data);

            _repo.Insert(objVoucher);
            try
            {
                await Request.GetContext().SaveChangesAsync();
                try
                {
                    if (objVoucher.VdrJson.Any())
                    {
                        _repo.ExecuteSql($"exec [Proc_GBL_GroupVoucher_ApiCalls] @TransactionId={objVoucher.Id},@TransactionNumber='{objVoucher.VoucherNo}',@TransactionType={objVoucher.ViewId.GetValueOrDefault()},@ActionType=0,@JsonData='{JsonConvert.SerializeObject(objVoucher.VdrJson)}'");
                    }
                }
                catch (SqlException ex)
                {
                    throw new BusinessException(ex.Message);
                }
            }
            catch (SqlException ex)
            {
                if (VoucherExists(objVoucher.VoucherNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                return BadRequest(ex.Message);
            }

            return Created(objVoucher);
        }
        //// PATCH: odata/Vouchers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<Voucher> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var jdata = patch.GetEntity().Data;
            Voucher objVoucher = await _repo.FindAsync(key);
            if (objVoucher == null)
            {
                return NotFound();
            }
            objVoucher.ObjectState = ObjectState.Modified;
            patch.Patch(objVoucher);

            try
            {
                try
                {
                    foreach (var entity in jdata)
                    {
                        objVoucher.DeleteAndAdd(entity);
                    }
                }
                catch
                {
                    //Ignore
                }

                await Request.GetContext().SaveChangesAsync();
                if (objVoucher.RemoveVD)
                {
                    await _repo.ExecuteSqlAsync($"DELETE FROM [dbo].[tVoucherVD] where VoucherId={objVoucher.Id}");
                }

                try
                {
                    if (objVoucher.VdrJson.Any())
                    {
                        _repo.ExecuteSql($"exec [Proc_GBL_GroupVoucher_ApiCalls] @TransactionId={objVoucher.Id},@TransactionNumber='{objVoucher.VoucherNo}',@TransactionType={objVoucher.ViewId.GetValueOrDefault()},@ActionType=1,@JsonData='{JsonConvert.SerializeObject(objVoucher.VdrJson)}'");
                    }
                }
                catch (SqlException ex)
                {
                    throw new BusinessException(ex.Message);
                }

                
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
            
            return Updated(objVoucher);
        }
        // DELETE: odata/Vouchers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVoucher = await _repo.FindAsync(key);
            if (objVoucher == null)
            {
                return NotFound();
            }
            try
            {
                _repo.ExecuteSql($"exec [Proc_GBL_GroupVoucher_ApiCalls] @TransactionId={objVoucher.Id},@TransactionNumber='',@TransactionType={objVoucher.ViewId.GetValueOrDefault()},@ActionType=3,@JsonData='[]'");
                objVoucher.ObjectState = ObjectState.Deleted;
                _repo.Delete(objVoucher);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ex.Message);
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        public IHttpActionResult UpdateAuditStatus([FromODataUri] long key, ODataActionParameters parameters)
        {

            var isAudited = (int)parameters["isAudited"];
            var remark = (string)parameters["remark"];
            if (key<=0) return BadRequest("Invalid Voucher id");
            var count=_repo.ExecuteSql($"UPDATE tVouchers SET AuditRemark='{remark}',IsAudited={isAudited} WHERE Id={key}");
            if (count > 0)
            {
                return Ok();
            }
            return BadRequest("Nothing to Update");
        }

        // POST: odata/CNBills(key)/BillLogs
        [AcceptVerbs("POST")]
        [ODataRoute("Vouchers({key})/VoucherDetails")]
        public async Task<IHttpActionResult> PostVoucherDetails([FromODataUri]long key, [FromBody] VoucherDetail vd)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var isVoucherExists =
                await _repo.Queryable().AnyAsync(x => x.Id == key);
            if (!isVoucherExists)
            {
                return NotFound();
            }
            vd.ObjectState=ObjectState.Added;
            vd.VoucherId = key;


            uow.RepositoryAsync<VoucherDetail>().Insert(vd);
            var _jsonVDRs = vd.JsonVDRS;
            await uow.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(_jsonVDRs) && (vd.VoucherDetailReferences?.Count ?? 0) == 0)
            {
                var vdrs = JsonConvert.DeserializeObject<List<VoucherDetailReference>>(_jsonVDRs);
                var transaction = uow.Context.Database.CurrentTransaction ??
                              uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);

                vdrs?.ForEach(x =>
                {
                    if (x.Id == 0)
                    {
                        x.ObjectState = ObjectState.Added;
                        x.VoucherDetailId = vd.Id;
                        x.fk_VoucherDetail = vd;
                        uow.RepositoryAsync<VoucherDetailReference>().Insert(x);
                    }
                    else
                    {
                        x.ObjectState = ObjectState.Modified;
                        x.VoucherDetailId = vd.Id;
                        x.fk_VoucherDetail = vd;
                        uow.RepositoryAsync<VoucherDetailReference>().Update(x);
                    }
                });
                //uow.BulkInsert(vdrs, transaction.UnderlyingTransaction);
                await uow.SaveChangesAsync();
            }

            return Created(vd);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool VoucherExists(string voucherNo)
        {
            return _repo.Query(e => e.VoucherNo == voucherNo).Select().Any();
        }
        private bool VoucherExists(long key)
        {
            return _repo.Query(e => e.Id == key).Select().Any();
        }
        
    }
}