using Repository.Pattern.Core.Repositories;

using System;
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
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;

using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.FMS
{
    public class GTransController : ODataController
    {
        private readonly IRepositoryAsync<GeneralTransaction> _gtransRepo;

        public GTransController(IRepositoryAsync<GeneralTransaction> service)
        {
            _gtransRepo = service;
        }
        // GET: odata/GTrans
        [HttpGet, EnableQuery]
        public IQueryable<GeneralTransaction> Get()
        {
            return _gtransRepo.Queryable();
        }
        // GET: odata/GTrans(5)
        [EnableQuery]
        public SingleResult<GeneralTransaction> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_gtransRepo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/GTrans(5)
        public async Task<IHttpActionResult> Put(long key, GeneralTransaction objGeneralTransaction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objGeneralTransaction.Id)
            {
                return BadRequest();
            }
            objGeneralTransaction.ObjectState = ObjectState.Modified;
            _gtransRepo.Update(objGeneralTransaction);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objGeneralTransaction);
        }
        // POST: odata/GTrans
        public async Task<IHttpActionResult> Post(GeneralTransaction objGeneralTransaction)
        {
            objGeneralTransaction.ObjectState = ObjectState.Added;
            _gtransRepo.Insert(objGeneralTransaction);
            await Request.GetContext().SaveChangesAsync();
            return Created(objGeneralTransaction);
        }
        //// PATCH: odata/GTrans(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GeneralTransaction> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GeneralTransaction objGeneralTransaction = await _gtransRepo.FindAsync(key);
            if (objGeneralTransaction == null)
            {
                return NotFound();
            }
            objGeneralTransaction.ObjectState = ObjectState.Modified;
            patch.Patch(objGeneralTransaction);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objGeneralTransaction);
        }
        // DELETE: odata/GTrans(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objGeneralTransaction = await _gtransRepo.FindAsync(key);
            if (objGeneralTransaction == null)
            {
                return NotFound();
            }
            await _gtransRepo.ExecuteSqlAsync($"DELETE FROM dbo.tGTransLog WHERE GenTranId={key}");
            objGeneralTransaction.ObjectState = ObjectState.Deleted;
            _gtransRepo.Delete(objGeneralTransaction);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        // POST: odata/VehicleMovementLogs(key)/Challans
        [ODataRoute("GTrans({key})/Logs")]
        public async Task<IHttpActionResult> PostLogs([FromODataUri] long key, [FromBody] GeneralTransLog log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _rpo = _gtransRepo.GetRepository<GeneralTransLog>();
            var item = _rpo.Insert(log);
            await _rpo.UOW.SaveChangesAsync();
            return Created(item);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var trans = await _gtransRepo.FindAsync(key);
            if (trans == null)
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
                    trans.VoucherId = newrecordid;
                    trans.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] long key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var triplog = await _gtransRepo.FindAsync(key);
            if (triplog == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_Voucher":
                    triplog.VoucherId = null;
                    triplog.fk_Voucher = null;
                    triplog.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
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
