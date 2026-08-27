using Repository.Pattern.Core.Repositories;

using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Service.FMS.GPS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.FMS.GPS
{
    [AuthorizeEx]
    public class IntrgrationServicesController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<IntrgrationServiceLog> _service;

        public IntrgrationServicesController(IRepositoryAsync<IntrgrationServiceLog> service)
        {
            _service = service;
        }
        // GET: odata/IntrgrationServices
        [HttpGet, EnableQuery]
        public IQueryable<IntrgrationServiceLog> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/IntrgrationServices(5)
        [EnableQuery]
        public SingleResult<IntrgrationServiceLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/IntrgrationServices(5)
        public async Task<IHttpActionResult> Put(long key, IntrgrationServiceLog objLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objLog.Id)
            {
                return BadRequest();
            }
            objLog.ObjectState = ObjectState.Modified;
            _service.Update(objLog);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objLog);
        }
        // POST: odata/IntrgrationServices
        public async Task<IHttpActionResult> Post(IntrgrationServiceLog objLog)
        {
            objLog.ObjectState = ObjectState.Added;
            _service.Insert(objLog);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objLog);
        }
        //// PATCH: odata/IntrgrationServices(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<IntrgrationServiceLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IntrgrationServiceLog objLog = await _service.FindAsync(key);

            if (objLog == null)
            {
                return NotFound();
            }
           
            objLog.ObjectState = ObjectState.Modified;
            patch.Patch(objLog);
            try
            {
                _service.Update(objLog);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objLog);
        }
        // DELETE: odata/IntrgrationServices(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objLog = await _service.FindAsync(key);
            if (objLog == null)
            {
                return NotFound();
            }
            //if (objDriverMaster.NextLogId.HasValue)
            //{
            //    throw new BusinessException(ErrorCode.GLB106,"Only Current Status can be deleted.");
            //}
            objLog.ObjectState = ObjectState.Deleted;
            _service.Delete(objLog);
            await Request.GetContext().SaveChangesAsync();
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
