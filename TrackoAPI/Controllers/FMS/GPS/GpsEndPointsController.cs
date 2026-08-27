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
    public class GpsEndPointsController : ODataController
    //ODataController
    {
        private readonly IGpsEndPointService _service;

        public GpsEndPointsController(IGpsEndPointService service)
        {
            _service = service;
        }
        // GET: odata/GpsEndPoints
        [HttpGet, EnableQuery]
        public IQueryable<GpsEndPoint> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/GpsEndPoints(5)
        [EnableQuery]
        public SingleResult<GpsEndPoint> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/GpsEndPoints(5)
        public async Task<IHttpActionResult> Put(long key, GpsEndPoint objGpsEndPoint)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objGpsEndPoint.Id)
            {
                return BadRequest();
            }
            objGpsEndPoint.ObjectState = ObjectState.Modified;
            _service.Update(objGpsEndPoint);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objGpsEndPoint);
        }
        // POST: odata/GpsEndPoints
        public async Task<IHttpActionResult> Post(GpsEndPoint objGpsEndPoint)
        {
            objGpsEndPoint.ObjectState = ObjectState.Added;
            _service.Insert(objGpsEndPoint);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objGpsEndPoint);
        }
        //// PATCH: odata/GpsEndPoints(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GpsEndPoint> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GpsEndPoint objGpsEndPoint = await _service.FindAsync(key);

            if (objGpsEndPoint == null)
            {
                return NotFound();
            }
           
            objGpsEndPoint.ObjectState = ObjectState.Modified;
            patch.Patch(objGpsEndPoint);
            try
            {
                _service.Update(objGpsEndPoint);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objGpsEndPoint);
        }
        // DELETE: odata/GpsEndPoints(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objGpsEndPoint = await _service.FindAsync(key);
            if (objGpsEndPoint == null)
            {
                return NotFound();
            }
            //if (objDriverMaster.NextLogId.HasValue)
            //{
            //    throw new BusinessException(ErrorCode.GLB106,"Only Current Status can be deleted.");
            //}
            objGpsEndPoint.ObjectState = ObjectState.Deleted;
            _service.Delete(objGpsEndPoint);
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
