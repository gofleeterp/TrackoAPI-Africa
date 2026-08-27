using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class SpareLogExtraInfosController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<SpareLogExtraInfo> _log;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public SpareLogExtraInfosController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWorkAsync = unitOfWorkAsync;
            _log = unitOfWorkAsync.RepositoryAsync<SpareLogExtraInfo>();
        }
        // GET: odata/SpareLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<SpareLogExtraInfo> Get()
        {
            return _log.Queryable();
        }
        // GET: odata/SpareLogs(5)
        [EnableQuery]
        public SingleResult<SpareLogExtraInfo> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_log.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/SpareLogs(5)
        public async Task<IHttpActionResult> Put(long key, SpareLogExtraInfo objSpareLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objSpareLog.Id)
            {
                return BadRequest();
            }
            objSpareLog.ObjectState = ObjectState.Modified;
            _log.Update(objSpareLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objSpareLog);
        }
        // POST: odata/SpareLogs
        public async Task<IHttpActionResult> Post(SpareLogExtraInfo objSpareLog)
        {
            objSpareLog.ObjectState = ObjectState.Added;
            _log.Insert(objSpareLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objSpareLog);
        }
        //// PATCH: odata/SpareLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SpareLogExtraInfo> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SpareLogExtraInfo objSpareLog = await _log.FindAsync(key);
            if (objSpareLog == null)
            {
                return NotFound();
            }
            objSpareLog.ObjectState = ObjectState.Modified;
            patch.Patch(objSpareLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Updated(objSpareLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSpareLog = await _log.FindAsync(key);
            if (objSpareLog == null)
            {
                return NotFound();
            }
            objSpareLog.ObjectState = ObjectState.Deleted;
            _log.Delete(objSpareLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
        [ODataRoute("SpareLogExtraInfos({key})/SpareLogs")]
        public async Task<IHttpActionResult> PostChallans([FromODataUri] long key, [FromBody] SpareLog spareLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            spareLog.ExtraInfoId = key;
            var uow = Request.GetContext();
            spareLog.ObjectState = ObjectState.Added;
            var item = uow.RepositoryAsync<SpareLog>().Insert(spareLog);
            await uow.SaveChangesAsync();
            return Created(item);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var sle = await _log.FindAsync(key);
            if (sle == null)
            {
                return NotFound();
            }
            var newrecordid = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_Voucher":
                    if (!uow.RepositoryAsync<Voucher>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    sle.VoucherId = newrecordid;
                    sle.ObjectState = ObjectState.Modified;
                    break;
                case "fk_TDSVoucher":
                    if (!uow.RepositoryAsync<Voucher>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    sle.TDSVoucherId = newrecordid;
                    sle.ObjectState = ObjectState.Modified;
                    break;
                case "fk_RelatedVoucher":
                    if (!uow.RepositoryAsync<Voucher>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    sle.RelatedVoucherId = newrecordid;
                    sle.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] long key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var sle = await _log.FindAsync(key);
            if (sle == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_Voucher":
                    sle.VoucherId = null;
                    sle.ObjectState = ObjectState.Modified;
                    break;
                case "fk_TDSVoucher":
                    sle.TDSVoucherId = null;
                    sle.ObjectState = ObjectState.Modified;
                    break;
                case "fk_RelatedVoucher":
                    sle.RelatedVoucherId = null;
                    sle.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}