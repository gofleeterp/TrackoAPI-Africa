using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DueInsuranceLogsController : ODataController
    //ODataController
    {
        private readonly IDueInsuranceLogService _objDueInsuranceLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DueInsuranceLogsController(IUnitOfWorkAsync unitOfWorkAsync, IDueInsuranceLogService service)
        {
            _objDueInsuranceLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/DueInsuranceLogs
        [HttpGet, EnableQuery]
        public IQueryable<DueInsuranceLog> Get()
        {
            return _objDueInsuranceLogService.Queryable();
        }
        // GET: odata/DueInsuranceLogs(5)
        [EnableQuery]
        public SingleResult<DueInsuranceLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDueInsuranceLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DueInsuranceLogs(5)
        public async Task<IHttpActionResult> Put(long key, DueInsuranceLog objDueInsuranceLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDueInsuranceLog.Id)
            {
                return BadRequest();
            }
            //objDueInsuranceLog.ObjectState = ObjectState.Modified;
            _objDueInsuranceLogService.Update(objDueInsuranceLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objDueInsuranceLog);
        }
        // POST: odata/DueInsuranceLogs
        public async Task<IHttpActionResult> Post(DueInsuranceLog objDueInsuranceLog)
        {
           // objDueInsuranceLog.ObjectState = ObjectState.Added;
            _objDueInsuranceLogService.Insert(objDueInsuranceLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objDueInsuranceLog);
        }
        //// PATCH: odata/DueInsuranceLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DueInsuranceLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DueInsuranceLog objDueInsuranceLog = await _objDueInsuranceLogService.FindAsync(key);
            if (objDueInsuranceLog == null)
            {
                return NotFound();
            }
            //objDueInsuranceLog.ObjectState = ObjectState.Modified;
            patch.Patch(objDueInsuranceLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objDueInsuranceLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDueInsuranceLog = await _objDueInsuranceLogService.FindAsync(key);
            if (objDueInsuranceLog == null)
            {
                return NotFound();
            }
            //objDueInsuranceLog.ObjectState = ObjectState.Deleted;
            _objDueInsuranceLogService.Delete(objDueInsuranceLog);
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
        
    }
}